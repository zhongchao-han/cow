using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;


public class DialoguePanelController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    private VisualElement root;
    private Image       avatarImage;
    private Label       nameLabel;
    private Label       contentLabel;

    void Update()
    {
        
    }

    void Awake()
    {
        UIDocumentDebugger.PrintVisualTree(uiDocument);
        var builder = uiDocument.rootVisualElement
                             .Query<VisualElement>("dialogue-panel");
    
    // Option 1: throws if not found
        root = builder.First();
        avatarImage = root.Q<Image>("avatar");
        nameLabel = root.Q<Label>("nameLabel");
        contentLabel = root.Q<Label>("contentLabel");

        // 默认隐藏
        root.style.display = DisplayStyle.None;
        // ShowDialogue(null, "sfdsfd", "sdfsdfsdddd");
    }

    /// <summary>
    /// 显示对话
    /// </summary>
    public void ShowDialogue(Sprite avatar, string characterName, string dialogueText)
    {
        avatarImage.sprite = avatar;
        nameLabel.text     = characterName;
        contentLabel.text  = dialogueText;
        root.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// 隐藏对话
    /// </summary>
    public void HideDialogue()
    {
        root.style.display = DisplayStyle.None;
    }
}


public static class UIDocumentDebugger
{
    /// <summary>
    /// 打印整个 UI Document 的 VisualElement 树。
    /// </summary>
    public static void PrintVisualTree(UIDocument uiDocument)
    {
        Debug.Log($"***********************");
        var root = uiDocument.rootVisualElement;
        Debug.Log($"<color=yellow>UI Document Tree (root: {root.name ?? "<no name>"})</color>");
        PrintElement(root, 0);
    }

    private static void PrintElement(VisualElement element, int depth)
    {
        // 缩进
        string indent = new string(' ', depth * 2);

        // 元素名称（如果没有 name，则显示类型）
        string name = string.IsNullOrEmpty(element.name)
            ? $"<{element.GetType().Name}>"
            : element.name;

        // class 列表
        string classes = element.GetClasses().Any()
            ? $"class=[{string.Join(",", element.GetClasses())}]"
            : "";

        Debug.Log($"{indent}- {element.GetType().Name} \"{name}\" {classes}");

        // 递归子节点
        foreach (var child in element.Children())
        {
            PrintElement(child, depth + 1);
        }
    }
}
