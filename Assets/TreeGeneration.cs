using UnityEngine;
using System.Collections.Generic;


public class TreeGeneration : MonoBehaviour
{
       void Start()
    {
        
    }

  
    void Update()
    {
        
    }
    
    void generateTree()
    {
        Node rootNode = new Node("Root", null);
        Node childNode1 = new Node("Child 1", rootNode);
        Node childNode2 = new Node("Child 2", rootNode);
        rootNode.childNodes.Add(childNode1);
        rootNode.childNodes.Add(childNode2);
    }

    
}

public class Node
{
    public string data;
    public Node parentNode;
    public List<Node> childNodes;

    public Node(string data, Node parentNode)
    {
        this.data = data;
        this.parentNode = parentNode;
        this.childNodes = new List<Node>();
    }



}

