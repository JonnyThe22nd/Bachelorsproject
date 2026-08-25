using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Baut und verwaltet den Baum.
/// </summary>
public class TreeMenuBuilder : MonoBehaviour
{
    [Header("Canvas")]
    public RectTransform canvasRoot;

    public RectTransform content;

    [Header("Prefabs")]

    public Button nodePrefab;

    public Image linePrefab;
    
    public Button plusButtonPrefab;

    public NamingPopup namingPopupPrefab;

    [Header("Erstellung")]

    public GameObject createPromptButton;

    public int maxEditableDepth = 2;

    [Header("Layout")]
    public HierarchicalTreeLayout layout = new HierarchicalTreeLayout();

    [Header("Auto-Fit")]
    [Range(0.1f, 1f)]
    public float fillPercent = 0.85f;
    public float maxScale = 1.5f;

    [Header("Verhalten")]
    public float lineThickness = 4f;

    MenuNode root;
    readonly List<MenuNode> allNodes = new List<MenuNode>();
    bool rootPopupOpen = false;

    public bool HasTree => root != null;

    void Awake()
    {
        EnsureContent();

        if (createPromptButton != null)
        {
            var btn = createPromptButton.GetComponentInChildren<Button>(true);
            if (btn != null) btn.onClick.AddListener(PromptCreateRoot);
            else Debug.LogWarning("TreeMenuBuilder: kein Button auf/unter createPromptButton gefunden.");
            createPromptButton.SetActive(false);
        }
    }

    void EnsureContent()
    {
        if (content != null) return;

        var go = new GameObject("TreeContent", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(canvasRoot, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        content = rt;
    }


    public void SetCreatePromptVisible(bool visible)
    {
        if (createPromptButton != null && !HasTree && !rootPopupOpen)
            createPromptButton.SetActive(visible);
    }

    public void PromptCreateRoot()
    {
        if (HasTree || namingPopupPrefab == null || rootPopupOpen) return;

        rootPopupOpen = true;
        if (createPromptButton != null) createPromptButton.SetActive(false);

        var popup = Instantiate(namingPopupPrefab, content);
        popup.Init(
            label => CreateRoot(label),
            "Name des Baums...",
            () => rootPopupOpen = false 
        );
    }

    public void CreateRoot(string label)
    {
        if (createPromptButton != null) createPromptButton.SetActive(false);

        var newRoot = new MenuNode(label);
        Build(newRoot);
    }

    public void RequestAddChild(MenuNode parent)
    {
        if (namingPopupPrefab == null) return;

        var popup = Instantiate(namingPopupPrefab, content);
        popup.Init(label =>
        {
            var child = parent.AddChild(label);
            CollectAndInstantiate(child);
            Relayout();
        }, "Name...");
    }

    public void Build(MenuNode treeRoot)
    {
        EnsureContent();
        Clear();
        root = treeRoot;
        CollectAndInstantiate(root);
        Relayout();
    }

    public void Clear()
    {
        if (content == null) return;
        foreach (Transform child in content)
            Destroy(child.gameObject);
        allNodes.Clear();
        root = null;
    }

    void CollectAndInstantiate(MenuNode node)
    {
        allNodes.Add(node);

        Button btn = Instantiate(nodePrefab, content);
        node.VisualInstance = btn.GetComponent<RectTransform>();

        var label = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null) label.text = node.Label;

        btn.onClick.AddListener(() => OnNodeSelected(node));

        if (node.Depth < maxEditableDepth && plusButtonPrefab != null)
        {
            Button plus = Instantiate(plusButtonPrefab, node.VisualInstance);
            var plusRT = plus.GetComponent<RectTransform>();
            plusRT.anchorMin = plusRT.anchorMax = new Vector2(1f, 0.5f);
            plusRT.pivot = new Vector2(0f, 0.5f);
            plusRT.anchoredPosition = new Vector2(10f, 0f);
            plus.onClick.AddListener(() => RequestAddChild(node));
        }

        if (node.Parent != null)
        {
            Image line = Instantiate(linePrefab, content);
            line.rectTransform.SetAsFirstSibling();
            node.ConnectionToParent = line.rectTransform;
        }

        foreach (var child in node.Children)
            CollectAndInstantiate(child);
    }

    void OnNodeSelected(MenuNode node)
    {
        node.OnSelect?.Invoke();
    }

    void Relayout()
    {
        layout.Layout(root, Vector2.zero);

        foreach (var node in allNodes)
        {
            bool visible = node.IsVisible();
            node.VisualInstance.gameObject.SetActive(visible);
            if (node.ConnectionToParent != null)
                node.ConnectionToParent.gameObject.SetActive(visible);
        }

        UpdateConnectionLines();
        FitContentToCanvas();
    }

    void UpdateConnectionLines()
    {
        foreach (var node in allNodes)
        {
            if (node.ConnectionToParent == null || node.Parent == null) continue;
            if (!node.VisualInstance.gameObject.activeSelf) continue;

            Vector2 from = node.Parent.VisualInstance.anchoredPosition;
            Vector2 to = node.VisualInstance.anchoredPosition;
            Vector2 delta = to - from;

            var rt = node.ConnectionToParent;
            rt.anchoredPosition = from;
            rt.sizeDelta = new Vector2(delta.magnitude, lineThickness);
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FitContentToCanvas()
    {
        bool any = false;
        float minX = 0, maxX = 0, minY = 0, maxY = 0;

        foreach (var node in allNodes)
        {
            if (!node.VisualInstance.gameObject.activeSelf) continue;
            Vector2 p = node.VisualInstance.anchoredPosition;

            if (!any) { minX = maxX = p.x; minY = maxY = p.y; any = true; }
            else
            {
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }
        }
        if (!any) return;

        RectTransform nodeRT = nodePrefab.GetComponent<RectTransform>();
        float nodeWidth = nodeRT.rect.width;
        float nodeHeight = nodeRT.rect.height;

        float boundsWidth = Mathf.Max(maxX - minX + nodeWidth, nodeWidth);
        float boundsHeight = Mathf.Max(maxY - minY + nodeHeight, nodeHeight);

        Rect canvasRect = canvasRoot.rect;
        float availW = canvasRect.width * fillPercent;
        float availH = canvasRect.height * fillPercent;

        float scale = Mathf.Min(availW / boundsWidth, availH / boundsHeight);
        scale = Mathf.Min(scale, maxScale);

        content.localScale = Vector3.one * scale;

        Vector2 center = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        content.anchoredPosition = -center * scale;
    }
}