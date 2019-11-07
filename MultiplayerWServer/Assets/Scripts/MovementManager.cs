using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{    
    [SerializeField] MapManager map;
    Node[] path;
    float t;
    List<Node> walkableNodes;

    private void Start()
    {
        walkableNodes = new List<Node>();
    }
    public Node[] GetPath(Transform start, Transform goal)
    {
        if (FindPath(goal, start))
        {
            return path;
        }
        else
            return null;

    }
    public List<Node> GetAllNodesUnderAScore(Transform start,int maxScore)
    {
        Node startingNode = map.GetNodeFromWorldPosition(start.position);
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
        return closedNodes;
    }

    bool FindPath(Transform objetive, Transform start)
    {
        Node startingNode = map.GetNodeFromWorldPosition(start.position);
        Node objetiveNode = map.GetNodeFromWorldPosition(objetive.position);
        List<Node> availableNodes = new List<Node>();
        List<Node> closedNodes = new List<Node>();
        availableNodes.Add(startingNode);
        Node currentNode = startingNode;
        currentNode.CalculateF(startingNode, objetiveNode.Pos);
        List<Node> currentNeighbords;
        Debug.Log("Debo llegar a" + objetiveNode.Pos);

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
                Debug.Log("Llegue a " + objetiveNode.Pos);
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

    public void DrawMovementRangeWSprint(Cell start)//recive player insted
    {            
        walkableNodes = GetAllNodesUnderAScore(start.transform, 92);
        foreach(Node node in walkableNodes)
        {
            if (node.g < 48)
            {
                map.getCellFromNode(node).SetWalkableColor();
            }
            else
            {
                map.getCellFromNode(node).SetSprintableColor();
            }
        }
    }
    public void DrawTurn1Movement(int team)
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
    }
    public void DrawMovementRangeWScore(Cell start,int score)//recive player insted
    {
        walkableNodes = GetAllNodesUnderAScore(start.transform, score);
        foreach (Node node in walkableNodes)
        {                
            map.getCellFromNode(node).SetWalkableColor();                
        }
    }
    public void ResetFloorColor()
    {
        foreach (Node node in walkableNodes)
        {
            map.getCellFromNode(node).SetBaseColor();
        }
    }
    public Node[] getCalculatedPath(Node startingNode, Node targetNode)
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
    
}
