using UnityEngine;

/// Lädt eine JSON-Datei und baut daraus einen laufzeitfähigen MenuNode-Baum.

public class MenuTreeJsonLoader : MonoBehaviour
{
  
    public TextAsset jsonFile;


    public SceneObjectRegistry sceneObjectRegistry;

    public TreeMenuBuilder builder;

    public bool autoBuildOnStart = true;

    void Start()
    {
        if (autoBuildOnStart)
            BuildFromJson();
    }

    [ContextMenu("Build From Json")]
    public void BuildFromJson()
    {
        var tree = LoadTree();
        if (tree != null)
            builder.Build(tree);
    }

    public MenuNode LoadTree()
    {
        if (jsonFile == null)
        {
            Debug.LogError("MenuTreeJsonLoader: kein JSON-File zugewiesen.");
            return null;
        }

        MenuNodeData data = JsonUtility.FromJson<MenuNodeData>(jsonFile.text);
        return ConvertToRuntimeTree(data);
    }

    MenuNode ConvertToRuntimeTree(MenuNodeData data)
    {
        var node = new MenuNode(data.label);

        if (!string.IsNullOrEmpty(data.sceneObjectId) && sceneObjectRegistry != null)
        {
            string id = data.sceneObjectId;
            node.OnSelect = () => sceneObjectRegistry.Show(id);
        }

        foreach (var childData in data.children)
            node.AddChild(ConvertToRuntimeTree(childData));

        return node;
    }
}
