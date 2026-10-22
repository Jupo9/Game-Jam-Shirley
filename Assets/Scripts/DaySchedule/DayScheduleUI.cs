using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("Game/UI/Day Schedule Club UI")]
public class DayScheduleUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DayCycleManager dayCycle;
    [Tooltip("Optional. If empty, the board builds its generated content inside this UI object.")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Table sizes (pixels)")]
    [SerializeField, Min(70f)] private float neighborColumnWidth = 135f;
    [SerializeField, Min(55f)] private float timeSlotColumnWidth = 90f;
    [SerializeField, Min(24f)] private float rowHeight = 36f;
    [SerializeField, Min(0f)] private float gapBetweenDays = 12f;

    [Header("Appearance")]
    [SerializeField] private Color cardColor = new Color(0.16f, 0.18f, 0.20f, 1f);
    [SerializeField] private Color columnHeaderColor = new Color(0.25f, 0.28f, 0.31f, 1f);
    [SerializeField] private Color currentSlotHeaderColor = new Color(0.24f, 0.46f, 0.64f, 1f);
    [SerializeField] private Color neighborCellColor = new Color(0.22f, 0.24f, 0.27f, 1f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color mutedTextColor = new Color(0.76f, 0.79f, 0.82f, 1f);

    private const string GeneratedRootName = "__GeneratedDayScheduleBoard";
    private RectTransform generatedRoot;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (dayCycle != null)
        {
            dayCycle.OnTimeChanged += RefreshBoard;
        }

        RefreshBoard();
    }

    private void OnDisable()
    {
        if (dayCycle != null)
        {
            dayCycle.OnTimeChanged -= RefreshBoard;
        }
    }

    // Call this after changing Active/VisibleOnBoard or slot data at runtime.
    public void RefreshBoard()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureGeneratedRoot();
        if (generatedRoot == null)
        {
            return;
        }

        // Destroy is deferred by Unity. Detach first so a second refresh in the
        // same frame never mixes previously generated cards with new cards.
        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedRoot.GetChild(i);
            child.gameObject.SetActive(false);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }

        if (dayCycle == null || !dayCycle.IsConfigured || dayCycle.DaySchedule == null)
        {
            CreateInfoCard("BOARD", "Assign a DayCycleManager with a configured DaySchedule.");
            return;
        }

        CreateDayCard(dayCycle.CurrentDayIndex, true);
        CreateDayCard(dayCycle.NextDayIndex, false);
    }

    /// <summary>Can be invoked later by an Open UI trigger or UI Manager.</summary>
    public void Open()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true); // OnEnable refreshes the board.
        }
        else
        {
            RefreshBoard();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void EnsureGeneratedRoot()
    {
        RectTransform parent = contentRoot != null ? contentRoot : (RectTransform)transform;
        if (generatedRoot != null && generatedRoot.parent == parent)
        {
            return;
        }

        Transform existing = parent.Find(GeneratedRootName);
        generatedRoot = existing != null
            ? existing as RectTransform
            : CreateUIObject(GeneratedRootName, parent);

        if (generatedRoot == null)
        {
            Debug.LogError("Board: generated root must be a RectTransform.", this);
            return;
        }

        Stretch(generatedRoot);
        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(generatedRoot.gameObject);
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = gapBetweenDays;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
    }

    private void CreateInfoCard(string title, string message)
    {
        RectTransform card = CreateCard("Board Information");
        CreateText(title, card, 22, textColor, 42, true);
        CreateText(message, card, 15, mutedTextColor, 80);
    }

    private void CreateDayCard(int dayIndex, bool isCurrentDay)
    {
        DaySchedule schedule = dayCycle.DaySchedule;
        string dayName = schedule.Days[dayIndex].Name;
        RectTransform card = CreateCard(isCurrentDay ? "Current Day" : "Next Day");

        CreateText(isCurrentDay ? "CURRENT DAY" : "NEXT DAY", card, 13, mutedTextColor, 22, true);
        CreateText(dayName, card, 24, textColor, 39, true);

        string subheading = isCurrentDay
            ? (dayCycle.IsDayComplete
                ? "Day complete  •  " + dayCycle.SlotsConsumedToday + "/" + dayCycle.TotalSlotsPerDay + " used"
                : dayCycle.CurrentSlotName + "  •  " + dayCycle.SlotsConsumedToday + "/" + dayCycle.TotalSlotsPerDay + " used")
            : "Upcoming schedule";
        CreateText(subheading, card, 14, mutedTextColor, 28);

        RectTransform scrollRoot = CreateUIObject("Schedule Scroll", card);
        LayoutElement scrollLayout = scrollRoot.gameObject.AddComponent<LayoutElement>();
        scrollLayout.minHeight = 80;
        scrollLayout.flexibleHeight = 1;

        ScrollRect scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = true;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 22f;

        RectTransform viewport = CreateUIObject("Viewport", scrollRoot);
        Stretch(viewport);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0f);
        viewportImage.raycastTarget = true; // Lets ScrollRect receive pointer drags.
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateUIObject("Table", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup rowsLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        rowsLayout.spacing = 4;
        rowsLayout.childAlignment = TextAnchor.UpperLeft;
        rowsLayout.childControlWidth = true;
        rowsLayout.childControlHeight = false;
        rowsLayout.childForceExpandWidth = false;
        rowsLayout.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = content.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;

        CreateTableHeader(content, schedule, isCurrentDay);

        int shownNeighbors = 0;
        for (int neighborIndex = 0; neighborIndex < schedule.Neighbors.Count; neighborIndex++)
        {
            if (!schedule.ShouldShowNeighborOnBoard(neighborIndex))
            {
                continue;
            }

            CreateNeighborRow(content, schedule, dayIndex, neighborIndex);
            shownNeighbors++;
        }

        if (shownNeighbors == 0)
        {
            RectTransform emptyRow = CreateTableRow("No visible neighbors", content, schedule.TimeSlots.Count);
            CreateTextCell("No active, board-visible neighbors", emptyRow,
                GetTableWidth(schedule.TimeSlots.Count), neighborCellColor, mutedTextColor, 13, false);
        }

        CreateText(BuildLegend(schedule), card, 12, mutedTextColor, 25);
    }

    private RectTransform CreateCard(string name)
    {
        RectTransform card = CreateUIObject(name, generatedRoot);
        Image background = card.gameObject.AddComponent<Image>();
        background.color = cardColor;
        background.raycastTarget = false;

        LayoutElement size = card.gameObject.AddComponent<LayoutElement>();
        size.minWidth = 0;
        size.flexibleWidth = 1;
        size.flexibleHeight = 1;

        VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return card;
    }

    private void CreateTableHeader(RectTransform parent, DaySchedule schedule, bool isCurrentDay)
    {
        RectTransform row = CreateTableRow("Time Slot Headers", parent, schedule.TimeSlots.Count);
        CreateTextCell("NEIGHBOR", row, neighborColumnWidth, columnHeaderColor, textColor, 12, true);

        for (int i = 0; i < schedule.TimeSlots.Count; i++)
        {
            bool highlight = isCurrentDay && dayCycle.CurrentSlotIndex == i;
            CreateTextCell(schedule.TimeSlots[i].Name, row, timeSlotColumnWidth,
                highlight ? currentSlotHeaderColor : columnHeaderColor, textColor, 12, true);
        }
    }

    private void CreateNeighborRow(RectTransform parent, DaySchedule schedule, int dayIndex, int neighborIndex)
    {
        NeighborDefinition neighbor = schedule.Neighbors[neighborIndex];
        RectTransform row = CreateTableRow(neighbor.Name, parent, schedule.TimeSlots.Count);
        CreateTextCell(neighbor.Name, row, neighborColumnWidth, neighborCellColor, textColor, 13, true);

        for (int slotIndex = 0; slotIndex < schedule.TimeSlots.Count; slotIndex++)
        {
            SlotStatusDefinition status = schedule.GetSlotStatus(dayIndex, neighborIndex, slotIndex);
            Color color = status != null ? status.Color : columnHeaderColor;
            string marker = status != null ? status.Marker : "?";
            CreateTextCell(marker ?? string.Empty, row, timeSlotColumnWidth, color,
                ReadableTextColor(color), 16, true);
        }
    }

    private RectTransform CreateTableRow(string name, RectTransform parent, int slotCount)
    {
        RectTransform row = CreateUIObject(name, parent);
        LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = GetTableWidth(slotCount);
        element.preferredHeight = rowHeight;

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row;
    }

    private float GetTableWidth(int slotCount)
    {
        return neighborColumnWidth + slotCount * timeSlotColumnWidth + slotCount * 4;
    }

    private void CreateTextCell(string value, RectTransform parent, float width, Color background,
        Color foreground, float fontSize, bool bold)
    {
        RectTransform cell = CreateUIObject("Cell", parent);
        Image image = cell.gameObject.AddComponent<Image>();
        image.color = background;
        image.raycastTarget = false;

        LayoutElement element = cell.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.preferredHeight = rowHeight;

        RectTransform textRect = CreateUIObject("Text", cell);
        Stretch(textRect, 6, 6, 0, 0);
        TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(text, value, fontSize, foreground, bold);
        text.alignment = TextAlignmentOptions.Midline;
    }

    private void CreateText(string value, RectTransform parent, float fontSize, Color foreground,
        float height, bool bold = false)
    {
        RectTransform rect = CreateUIObject("Label", parent);
        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = height;

        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(text, value, fontSize, foreground, bold);
        text.alignment = TextAlignmentOptions.MidlineLeft;
    }

    [System.Obsolete]
    private void ConfigureText(TextMeshProUGUI text, string value, float fontSize,
        Color foreground, bool bold)
    {
        text.text = value;
        text.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = foreground;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
    }

    private static string BuildLegend(DaySchedule schedule)
    {
        StringBuilder legend = new StringBuilder("Legend: ");
        for (int i = 0; i < schedule.SlotStatus.Count; i++)
        {
            SlotStatusDefinition status = schedule.SlotStatus[i];
            if (i > 0)
            {
                legend.Append("   |   ");
            }

            string marker = string.IsNullOrEmpty(status.Marker) ? "—" : status.Marker;
            legend.Append(marker).Append(" = ").Append(status.Name);
        }
        return legend.ToString();
    }

    private static Color ReadableTextColor(Color background)
    {
        float brightness = background.r * 0.299f + background.g * 0.587f + background.b * 0.114f;
        return brightness > 0.6f ? Color.black : Color.white;
    }

    private static RectTransform CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void Stretch(RectTransform rect, float left = 0f, float right = 0f,
        float top = 0f, float bottom = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
