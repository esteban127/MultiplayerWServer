using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapManager : MonoBehaviour
{
    GameObject[,] cells;
    Node[,] nodes;
    Dictionary<Character, Vector2> characterPos;
    int size = 19;
    [SerializeField] GameObject cellPrefab;

    string[,] cellInfo;

    private void Awake()
    {
        GenerateMap();
        characterPos = new Dictionary<Character, Vector2>();
    }    

    public void AddPlayer(Character player, Vector2 pos)
    {
        characterPos.Add(player, pos);
    }
    public void ActualziatePlayerPos(Character player, Vector2 pos)
    {
        characterPos[player] = pos;
    }
    public void ResetPlayersPos()
    {
        List<Character> keys = new List<Character>(characterPos.Keys);
        foreach(Character key in keys)
        {
            characterPos[key] = new Vector2(-1, -1);
        }
    }
    public Character GetCharacterByPos(Vector2 pos,bool onlyVisible)
    {
        foreach(Character player in characterPos.Keys)
        {
            if (characterPos[player] == pos && (player.Visible || !onlyVisible))
                return player;
        }
        return null;
    }

    public void GenerateMap()
    {
        LoadJsonMap();
        cells = new GameObject[size, size];
        nodes = new Node[size, size];        
        List<CellType> totalTypes;
        bool walkeable;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {        
                GameObject cell = Instantiate(cellPrefab, new Vector3(i, 0, j), new Quaternion(), transform);
                totalTypes = new List<CellType>();
                string[] info = cellInfo[i, j].Split(new char[] { '-' });
                walkeable = true;
                foreach(string type in info)
                {
                    switch (type)
                    {
                        case "b":
                            totalTypes.Add(CellType.Block);
                            walkeable = false;
                            break;
                        case "be":
                            totalTypes.Add(CellType.BuffEnergize);
                            break;
                        case "bs":
                            totalTypes.Add(CellType.BuffHaste);
                            break;
                        case "bh":
                            totalTypes.Add(CellType.BuffHeal);
                            break;
                        case "bm":
                            totalTypes.Add(CellType.BuffMight);
                            break;
                        case "e":
                            totalTypes.Add(CellType.Empty);
                            break;
                        case "f":
                            totalTypes.Add(CellType.Fog);
                            break;
                        case "u":
                            totalTypes.Add(CellType.Unwalkable);
                            walkeable = false;
                            break;
                        case "wc":
                            totalTypes.Add(CellType.Wcover);
                            break;
                        case "nc":
                            totalTypes.Add(CellType.Ncover);
                            break;
                        case "ec":
                            totalTypes.Add(CellType.Ecover);
                            break;
                        case "sc":
                            totalTypes.Add(CellType.Scover);
                            break;
                        case "wb":
                            totalTypes.Add(CellType.Wblock);
                            break;
                        case "nb":
                            totalTypes.Add(CellType.Nblock);
                            break;
                        case "eb":
                            totalTypes.Add(CellType.Eblock);
                            break;
                        case "sb":
                            totalTypes.Add(CellType.Sblock);
                            break;
                    }
                }     
                cell.GetComponent<Cell>().cellType = totalTypes;
                cell.GetComponent<Cell>().Pos = new Vector2(i, j);
                nodes[i, j] = new Node(new Vector2(i, j), walkeable);
                cell.name = (i + " , " + j);
                GameDirector.turnStart += cell.GetComponent<Cell>().NewTurn;
                cells[i, j] = cell;
            }
        }
    }
    public List<Node> GetAllNodesInARange(Node currentNode, int range)
    {
        List<Node> neighbords = new List<Node>();
        Vector2 coord = currentNode.Pos;
        int x;
        int y;
        for (int i = -range; i <= range; i++)
        {
            x = (int)coord.x + i;
            for (int j = -range; j <= range; j++)
            {
                y = (int)coord.y + j;
                if (CheckAvialableNode(x, y))
                    neighbords.Add(nodes[x, y]);
            }
        }
        return neighbords;
    }

    public List<Node> GetNeighborsNodes(Node currentNode,int team, bool onlyVisible)
    {
        List<Node> neighbords = new List<Node>();
        Vector2 coord = currentNode.Pos;
        int x;
        int y;
        for (int i = -1; i <= 1; i++)
        {
            x = (int)coord.x + i;
            for (int j = -1; j <= 1; j++)
            {
                y = (int)coord.y+j;
                if(Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1)
                {
                    if (CheckAvialableNode(x-i, y)&& CheckAvialableNode(x, y-j))
                        if (CheckAvialableNode(x, y,team,onlyVisible))
                            neighbords.Add(nodes[x, y]);
                }
                else
                {
                    if (CheckAvialableNode(x, y,team,onlyVisible))
                        neighbords.Add(nodes[x, y]);
                }                
            }
        }
        return neighbords;
    }
    public List<Node> GetNeighborsNodes(Node currentNode)
    {
        List<Node> neighbords = new List<Node>();
        Vector2 coord = currentNode.Pos;
        int x;
        int y;
        for (int i = -1; i <= 1; i++)
        {
            x = (int)coord.x + i;
            for (int j = -1; j <= 1; j++)
            {
                y = (int)coord.y + j;
                if (Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1)
                {
                    if (CheckAvialableNode(x - i, y) && CheckAvialableNode(x, y - j))
                        if (CheckAvialableNode(x, y))
                            neighbords.Add(nodes[x, y]);
                }
                else
                {
                    if (CheckAvialableNode(x, y))
                        neighbords.Add(nodes[x, y]);
                }
            }
        }
        return neighbords;
    }

    public bool CheckAvialableNode(int posX, int posY)
    {
        if (posX >= 0 && posY >= 0 && posX <= size - 1 && posY <= size - 1)
        {
            return nodes[posX, posY].Walkable;
        }
        return false;
    }
    public bool CheckAvialableNode(int posX,int posY, int team, bool onlyVisible)
    {
        if (posX >= 0 && posY >= 0 && posX <= size - 1 && posY <= size - 1)
        {
            Character character = GetCharacterByPos(new Vector2(posX, posY), onlyVisible);
            if (character != null)
            {
                bool alied = character.Team == team;
                return nodes[posX, posY].Walkable && alied;
            }
            else return nodes[posX, posY].Walkable;
        }
        return false;
    }
    

    private void LoadJsonMap()
    {
        cellInfo = new string[size, size];
        int count = 0;
        TextAsset text = Resources.Load<TextAsset>("Map/data");
        JsonMap rawJsonMap = JsonUtility.FromJson<JsonMap>(text.ToString());
        foreach (JsonLine line in rawJsonMap.Lines)
        {
            cellInfo[count, 0] = line.R0;
            cellInfo[count, 1] = line.R1;
            cellInfo[count, 2] = line.R2;
            cellInfo[count, 3] = line.R3;
            cellInfo[count, 4] = line.R4;
            cellInfo[count, 5] = line.R5;
            cellInfo[count, 6] = line.R6;
            cellInfo[count, 7] = line.R7;
            cellInfo[count, 8] = line.R8;
            cellInfo[count, 9] = line.R9;
            cellInfo[count, 10] = line.R10;
            cellInfo[count, 11] = line.R11;
            cellInfo[count, 12] = line.R12;
            cellInfo[count, 13] = line.R13;
            cellInfo[count, 14] = line.R14;
            cellInfo[count, 15] = line.R15;
            cellInfo[count, 16] = line.R16;
            cellInfo[count, 17] = line.R17;
            cellInfo[count, 18] = line.R18;            
            count++;        
        }
    }

    public void DestroyCells()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (Application.isEditor)
                DestroyImmediate(transform.GetChild(childCount-i-1).gameObject);
        }
    }
    public Cell GetCellFromWorldPosition(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < size && pos.z >= 0 && pos.z < size)
            return cells[(int)pos.x, (int)pos.z].GetComponent<Cell>();


        Debug.LogError("there is no cell for the pos" + (int)pos.x + ", " + (int)pos.z);
        return null;
    }
    public Cell GetCellFromCoord(Vector2 coord)
    {
        if (coord.x >= 0 && coord.x < size && coord.y >= 0 && coord.y < size)
            return cells[(int)coord.x, (int)coord.y].GetComponent<Cell>();
        
        return null;
    }
    public Node GetNodeFromWorldPosition(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < size && pos.z >= 0 && pos.z < size)
            return nodes[(int)pos.x, (int)pos.z];


        Debug.LogError("there is no node for the pos" + (int)pos.x + ", " + (int)pos.z);
        return null;
    }
    public Node GetNodeFromACoord(Vector2 coord)
    {        
        if (coord.x >= 0 && coord.x < size && coord.y >= 0 && coord.y < size)
            return nodes[(int)coord.x, (int)coord.y];


        Debug.LogError("there is no node for the pos" + (int)coord.x + ", " + (int)coord.y);
        return null;
    }
    public Cell getCellFromNode(Node node)
    {
        return cells[(int)node.Pos.x,(int)node.Pos.y].GetComponent<Cell>();
    }
    public Vector2 GetSpawnBaseSpawnPoint(int team)
    {
        Vector2 spawn;
        spawn = team == 0 ? new Vector2(2, 9) : new Vector2(16, 9);
        return spawn;
    }
}

[System.Serializable]
public class JsonMap
{
    public JsonLine[] Lines;
}
[System.Serializable]
public class JsonLine
{
    public string R0;
    public string R1;
    public string R2;
    public string R3;
    public string R4;
    public string R5;
    public string R6;
    public string R7;
    public string R8;
    public string R9;
    public string R10;
    public string R11;
    public string R12;
    public string R13;
    public string R14;
    public string R15;
    public string R16;
    public string R17;
    public string R18;
}

