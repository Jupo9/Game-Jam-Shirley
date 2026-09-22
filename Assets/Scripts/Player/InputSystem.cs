using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    [Header("Player Configs")]
    [Tooltip("Player Movement Speed")]
    [SerializeField] private float playerMovementSpeed;

    [Header("Item Interaction")]
    [SerializeField] private PlayerItemController playerItemController;

    [Header("Building Entrance Configs")]
    [Tooltip("Home Container")]
    [SerializeField] private GameObject homeUI;

    [Tooltip("Player stays on entrance Y/N")]
    [SerializeField] private bool staysOnEntrance = false;

    private bool activeUI = false;

    private void Start()
    {
        if (homeUI != null)
        {
            homeUI.SetActive(false);
        }
    }

    private void Update()
    {
        HandlePlayerMovement();
        HandlePlayerEntranceInteraction();
        HandlePlayerItemInteraction();
    }

    private void HandlePlayerMovement()
    {
        Vector3 movementDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            movementDirection += Vector3.forward;
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            movementDirection += Vector3.back;
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            movementDirection += Vector3.left;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            movementDirection += Vector3.right;
        }

        transform.position += movementDirection * playerMovementSpeed * Time.deltaTime;
    }

    private void HandlePlayerEntranceInteraction()
    {
        if (Keyboard.current.eKey.isPressed && staysOnEntrance)
        {
            homeUI.SetActive(true);                 // has to chance for diffrent panels based on the entrance!
            activeUI = true;
            Time.timeScale = 0f;
        }
    }

    private void HandlePlayerItemInteraction()
    {
        if (playerItemController == null)
        {
            Debug.Log("Return");
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("F Key was pressed");
            playerItemController.TryPickupItem();
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Debug.Log("G Key was pressed");
            playerItemController.DropEquippedItem();
        }
    }

    public void EnterEntranceTrigger()
    {
        staysOnEntrance = true;
    }

    public void ExitEntranceTrigger()
    {
        staysOnEntrance = false;
    }
}

