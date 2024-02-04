using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPickedSomething = null;
    }

    public event EventHandler<OnIngredientAddedArgs> OnIngredientAdded;
    public class OnIngredientAddedArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectList;

    private void Awake()
    {
        kitchenObjectList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            // Not a valid ingredient
            return false;
        }

        if (kitchenObjectList.Contains(kitchenObjectSO))
        {
            // Already has this type
            return false;
        }
        else
        {
            kitchenObjectList.Add(kitchenObjectSO);

            OnIngredientAdded?.Invoke(this, new OnIngredientAddedArgs
            {
                kitchenObjectSO = kitchenObjectSO
            });

            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);

            return true;
        }
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectList;
    }
}
