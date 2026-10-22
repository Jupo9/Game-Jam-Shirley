using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary IMGUI bag viewer: press I, click a seed to select it.
/// The real bag UI will replace this later without changing PlayerInventory.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Game/Debug/Player Inventory Debug UI")]
public class PlayerInventoryDebugUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private bool startOpen;

    [Header("Optional test item")]
    [SerializeField] private ItemData testItem;
    [SerializeField, Min(1)] private int testQuantity = 5;

    [Header("Window")]
    [SerializeField] private Vector2 startPosition = new(360f, 15f);

    private bool isOpen;
    private Rect windowRect;
    private Vector2 scrollPosition;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        isOpen = startOpen;
        windowRect = new Rect(startPosition.x, startPosition.y, 325f, 315f);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }
    }

    private void OnGUI()
    {
        if (!isOpen)
        {
            return;
        }

        windowRect.height = Mathf.Min(315f, Mathf.Max(175f, Screen.height - 10f));
        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, string.Empty);
        windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, Screen.width - windowRect.width));
        windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, Screen.height - windowRect.height));
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("SEED BAG - DEBUG", GUILayout.ExpandWidth(true));
        if (GUILayout.Button("X", GUILayout.Width(28f)))
        {
            isOpen = false;
        }
        GUILayout.EndHorizontal();

        if (inventory == null)
        {
            GUILayout.Label("PlayerInventory reference missing.");
            GUI.DragWindow(new Rect(0f, 0f, windowRect.width - 40f, 26f));
            return;
        }

        string selectedName = inventory.SelectedSeed != null
            ? inventory.SelectedSeed.ItemName
            : "None";
        GUILayout.Label("Selected seed: " + selectedName);
        GUILayout.Space(5f);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        if (inventory.Stacks.Count == 0)
        {
            GUILayout.Label("Bag is empty.");
        }

        foreach (BagItemStack stack in inventory.Stacks)
        {
            if (stack.Item == null)
            {
                continue;
            }

            bool isSeed = stack.Item.Category == ItemCategory.Seed;
            bool selected = stack.Item == inventory.SelectedSeed;
            string label = (selected ? "> " : "") + stack.Item.ItemName +
                           "  x" + stack.Amount + (isSeed ? " [Seed]" : "");

            if (isSeed)
            {
                if (GUILayout.Button(label))
                {
                    inventory.SelectSeed(stack.Item);
                }
            }
            else
            {
                GUILayout.Label(label);
            }
        }
        GUILayout.EndScrollView();

        if (testItem != null)
        {
            if (GUILayout.Button("Add test item x" + testQuantity))
            {
                if (!inventory.AddItem(testItem, testQuantity))
                {
                    Debug.LogWarning("Test item must be a bag item and quantity must be valid.", this);
                }
            }
        }

        GUILayout.Label("I: open / close    |    Click seeds to select");
        GUI.DragWindow(new Rect(0f, 0f, windowRect.width - 40f, 26f));
    }
}
