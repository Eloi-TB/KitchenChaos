using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMarsk;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private const float PLAYER_RADIUS = .7f;
    private const float PLAYER_HEIGHT = 2f;
    private const float INTERACT_DISTANCE = 2f;
    private float moveDistance = 0f;
    private bool canMove = true;
    private bool isWalking;
    private RaycastHit raycastHit;
    private Vector3 lastInteractDirection;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one Player instance");
        }
        Instance = this;
    }


    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        // if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        HandleMovement();
        HandleInteracions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteracions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        if (Physics.Raycast(transform.position, lastInteractDirection, out raycastHit, INTERACT_DISTANCE, countersLayerMarsk))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                // Has ClearCounter
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);

            }
        }
        else
        {
            SetSelectedCounter(null);

        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        isWalking = moveDirection != Vector3.zero;

        moveDistance = moveSpeed * Time.deltaTime;
        canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * PLAYER_HEIGHT, PLAYER_RADIUS, moveDirection, moveDistance);

        if (!canMove)
        {
            //Cannot move towards moveDirection

            // Attempt only X movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = moveDirectionX.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * PLAYER_HEIGHT, PLAYER_RADIUS, moveDirectionX, moveDistance);

            if (canMove)
            {
                // Can move only direction X
                moveDirection = moveDirectionX;
            }
            else
            {
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = moveDirectionZ.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * PLAYER_HEIGHT, PLAYER_RADIUS, moveDirectionZ, moveDistance);
                if (canMove)
                {
                    // Can move only on the Z
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    // Cannot move in any direction
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        isWalking = moveDirection != Vector3.zero;

        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed); //TODO sembla que dona un error cada poc
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
