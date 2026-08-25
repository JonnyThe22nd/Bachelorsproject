using System.Collections.Generic;


/// Reine Datenklasse für den JSON-Import. 
[System.Serializable]
public class MenuNodeData
{
    public string label;

    public string sceneObjectId;

    public List<MenuNodeData> children = new List<MenuNodeData>();
}
