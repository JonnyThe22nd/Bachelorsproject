using System.Collections.Generic;
using UnityEngine;

/// Verknüpft String-IDs  mit echten GameObjects in der Szene. 
public class SceneObjectRegistry : MonoBehaviour
{
    [System.Serializable]
    public class Link
    {
        
        public string id;
        public GameObject target;
    }

    
    public List<Link> links = new List<Link>();

   
    public bool hideOthersOnShow = true;

    Dictionary<string, GameObject> lookup;

    void Awake()
    {
        lookup = new Dictionary<string, GameObject>();
        foreach (var link in links)
        {
            if (string.IsNullOrEmpty(link.id) || link.target == null) continue;
            lookup[link.id] = link.target;
        }
    }

    public void Show(string id)
    {
        if (!lookup.TryGetValue(id, out var target))
        {
            Debug.LogWarning($"SceneObjectRegistry: keine Verknüpfung für id '{id}' gefunden.");
            return;
        }

        if (hideOthersOnShow)
            foreach (var kv in lookup)
                kv.Value.SetActive(kv.Key == id);
        else
            target.SetActive(true);
    }

    public GameObject Get(string id)
    {
        lookup.TryGetValue(id, out var go);
        return go;
    }
}
