using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float fryingTimer;
    private FryingRecipeSO fryingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });

                    if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                    {
                        // Fried
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state = State.Fried;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });
                    }
                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });

                    if (burningTimer > burningRecipeSO.burningTimerMax)
                    {
                        // Burned
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Player carring something that can be Fried
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    state = State.Frying;
                    fryingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = state
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                }
            }
            else
            {
                // Player not carring anything
            }
        }
        else
        {
            // There is a KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carring something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // Player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                // Player is not carring anithing
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = state
                });

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSo = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSo != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSo = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        if (fryingRecipeSo != null)
        {
            return fryingRecipeSo.output;
        }
        else
        {
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSo in fryingRecipeSOArray)
        {
            if (fryingRecipeSo.input == inputKitchenObjectSO)
            {
                return fryingRecipeSo;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
}
