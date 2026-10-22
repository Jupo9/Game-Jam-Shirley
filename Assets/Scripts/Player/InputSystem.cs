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
    [SerializeField] private GameUIManager uiManager;

    private bool activeUI = false;

    private void Awake()
    {
        if (playerItemController == null)
        {
            playerItemController = GetComponent<PlayerItemController>();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || (uiManager != null && uiManager.IsOpen))
        {
            return;
        }

        HandlePlayerMovement(keyboard);

        if (keyboard.fKey.wasPressedThisFrame && playerItemController != null)
        {
            playerItemController.DropEquippedItem();
        }
    }

    private void HandlePlayerMovement(Keyboard keyboard)
    {
        Vector3 movementDirection = Vector3.zero;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movementDirection += Vector3.forward;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movementDirection += Vector3.back;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movementDirection += Vector3.left;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movementDirection += Vector3.right;

        // Keep the movement behavior of the existing prototype.
        transform.position += movementDirection * playerMovementSpeed * Time.deltaTime;
    }
}

