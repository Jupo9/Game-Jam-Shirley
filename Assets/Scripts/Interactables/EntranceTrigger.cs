using System;
using UnityEngine;
using UnityEngine.UI;

public class EntranceTrigger : MonoBehaviour
{
    [Header("Entrance Indicator")]
    [SerializeField] private Image entranceIndicator;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = Color.black;
    [SerializeField] private Color activeColor = Color.green;

    [SerializeField] private InputSystem inputSystem;

    private void Start()
    {
        SetIndicator(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the trigger area.");

            if (entranceIndicator != null)
            {
                entranceIndicator.color = activeColor;
            }

            inputSystem.EnterEntranceTrigger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has exited the trigger area.");

            if (entranceIndicator != null)
            {
                entranceIndicator.color = inactiveColor;
            }

            inputSystem.ExitEntranceTrigger();
        }
    }

    public void SetIndicator(bool isActive)
    {
        if (entranceIndicator == null)
        { 
            return; 
        }

        if (isActive)
        {
            entranceIndicator.color = activeColor;
        }
        else
        {
            entranceIndicator.color = inactiveColor;
        }
    }
}
