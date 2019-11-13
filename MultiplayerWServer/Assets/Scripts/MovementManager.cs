using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{    
    [SerializeField] MapManager map;
    Node[] path;
    float t;
    List<Node> walkableNodes;
    List<Node> sprintableNodes;
    Node startingNode;
    
    private void Start()
    {
        walkableNodes = new List<Node>();
        sprintableNodes = new List<Node>();        
    }
    public Node[] GetPath(Node start, Node goal)
    {
        if (FindPath(goal, start))
        {
            return path;
        }
        else
            return null;

    }
    public List<Node> GetAllNodesUnderAScore(Node start,int maxScore)
    {
        Node startingNode = start;
        List<Node> availableNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();
        availableNodes.Add(startingNode);
        Node currentNode = startingNode;
        currentNode.CalculateOnlyG(startingNode);
        List<Node> currentNeighbords;

        while (availableNodes.Count > 0)
        {
            currentNode = availableNodes[0];

            availableNodes.Remove(currentNode);
            closedNodes.Add(currentNode);                               

            currentNeighbords = map.GetNeighborsNodes(currentNode);

            for (int i = 0; i < currentNeighbords.Count; i++)
            {
                if (availableNodes.Contains(currentNeighbords[i]))
                {                    
                    Node lastNode = currentNeighbords[i].Parent;
                    int neighbordG = currentNeighbords[i].g;
                    currentNeighbords[i].CalculateOnlyG(currentNode);
                    if (currentNeighbords[i].g > neighbordG)
                    {
                        currentNeighbords[i].CalculateOnlyG(lastNode);
                    }
                }
                else if (!closedNodes.Contains(currentNeighbords[i]))
                {
                    currentNeighbords[i].CalculateOnlyG(currentNode);
                    if(currentNeighbords[i].g <= maxScore)
                    availableNodes.Add(currentNeighbords[i]);
                }
            }

        }
        closedNodes.Remove(start);
        return closedNodes;
    }

    public List<Node> GetAllNodesBetweenScores(Node start, int minScore, int maxScore)
    {
        List<Node> totalNodes = GetAllNodesUnderAScore(start, maxScore);
        List<Node> finalNodes = new List<Node>();
        foreach(Node node in totalNodes)
        {
            if (node.g >= minScore)
            {
                finalNodes.Add(node);
            }
        }
        return finalNodes;
    }
    bool FindPath(Node objetiveNode, Node startingNode)
    {
        List<Node> availableNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();
        availableNodes.Add(startingNode);
        Node currentNode = startingNode;
        currentNode.CalculateF(startingNode, objetiveNode.Pos);
        List<Node> currentNeighbords;

        while (availableNodes.Count > 0)
        {
            currentNode = availableNodes[0];
            for (int i = 1; i < availableNodes.Count; i++)
            {
                if (availableNodes[i].f <= currentNode.f)
                {
                    if (availableNodes[i].h < currentNode.h)
                    {
                        currentNode = availableNodes[i];
                    }
                }
            }

            availableNodes.Remove(currentNode);
            closedNodes.Add(currentNode);
            //Debug.Log("me quedan " + nodosHabilitados.Count + " Nodos");

            if (currentNode == objetiveNode)
            {
                while (currentNode != startingNode)
                {
                    CalculatePath(objetiveNode, startingNode);
                    return true;
                }

            }

            currentNeighbords = map.GetNeighborsNodes(currentNode);

            for (int i = 0; i < currentNeighbords.Count; i++)
            {
                if (availableNodes.Contains(currentNeighbords[i]))
                {                      
                    Node lastNode = currentNeighbords[i].Parent;
                    currentNeighbords[i].CalculateF(currentNode, objetiveNode.Pos);
                    if (currentNeighbords[i].f > currentNode.f)
                    {
                        currentNeighbords[i].CalculateF(lastNode, objetiveNode.Pos);
                    }
                }
                else if (!closedNodes.Contains(currentNeighbords[i]))
                {
                    currentNeighbords[i].CalculateF(currentNode, objetiveNode.Pos);
                    //Debug.Log("agrego el nodo nuevo con f = " + currentNeighbords[i].f);
                    availableNodes.Add(currentNeighbords[i]);
                }
            }
        }
        return false;
    }

    private void CalculatePath(Node objetiveNode, Node startingNode)
    {
        List<Node> provitionalList = new List<Node>();
        Node currentNode = objetiveNode;

        while (currentNode != startingNode)
        {
            provitionalList.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path = new Node[provitionalList.Count];
        for (int i = 0; i < provitionalList.Count; i++)
        {
            path[path.Length - (i + 1)] = provitionalList[i];
        }
    }

    public void DrawMovementSprintRange(Node start, int sprintScore, int walkableScore)
    {
        sprintableNodes = GetAllNodesBetweenScores(start, walkableScore, sprintScore);
        foreach(Node node in sprintableNodes)
        {
            map.getCellFromNode(node).SetSprintableColor();            
        }
    }
    public void DrawMovementWalkableRange(Node start, int walkableScore)
    {
        walkableNodes = GetAllNodesUnderAScore(start, walkableScore);
        startingNode = start;
        map.getCellFromNode(start).SetCurrentCellColor();
        foreach (Node node in walkableNodes)
        {            
            map.getCellFromNode(node).SetWalkableColor();            
        }
    }
    public void DrawTurn1Movement(int team, Node start)
    {        
        Vector2 center = map.GetSpawnBaseSpawnPoint(team);
        center.x += team == 0 ? 1 : -1;
        List<Node> centerNodes = map.GetAllNodesInARange(map.GetNodeFromACoord(center), 5);
        center.x += team == 0 ? 1 : -1;
        center.y += 8;
        List<Node> rigthNodes = map.GetAllNodesInARange(map.GetNodeFromACoord(center), 2);
        center.y -= 16;
        List<Node> leftNodes = map.GetAllNodesInARange(map.GetNodeFromACoord(center), 2);
        foreach(Node node in centerNodes)
        {
            walkableNodes.Add(node);
        }
        foreach (Node node in rigthNodes)
        {
            walkableNodes.Add(node);
        }
        foreach (Node node in leftNodes)
        {
            walkableNodes.Add(node);
        }
        foreach (Node node in walkableNodes)
        {            
            map.getCellFromNode(node).SetWalkableColor();           
        }
        startingNode = start;
        map.getCellFromNode(startingNode).SetCurrentCellColor();
    }
    public void DrawMovementRangeWScore(Node start,int score)//recive player insted
    {
        walkableNodes = GetAllNodesUnderAScore(start, score);
        foreach (Node node in walkableNodes)
        {                
            map.getCellFromNode(node).SetWalkableColor();                
        }
    }
    public void ResetFloorColor()
    {
        map.getCellFromNode(startingNode).SetBaseColor();
        foreach (Node node in walkableNodes)
        {
            map.getCellFromNode(node).SetBaseColor();
        }
        foreach (Node node in sprintableNodes)
        {
            map.getCellFromNode(node).SetBaseColor();
        }
    }
    public Node[] GetCalculatedPath(Node startingNode, Node targetNode)
    {
        List<Node> provitionalList = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startingNode)
        {
            provitionalList.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        Node[] calculatedPath = new Node[provitionalList.Count];
        for (int i = 0; i < provitionalList.Count; i++)
        {
            calculatedPath[calculatedPath.Length - (i + 1)] = provitionalList[i];
        }
        return calculatedPath;
    }
    public bool WalkableNode(Node node)
    {
        return walkableNodes.Contains(node);
    }
    public bool SprintableNode(Node node)
    {
        return sprintableNodes.Contains(node);
    }
}
