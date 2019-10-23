using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapManager : MonoBehaviour
{
    GameObject[,] cells;
    int size = 19;
    [SerializeField] GameObject cellPrefab;

    string[,] cellInfo;

    public void GenerateMap()
    {
        LoadJsonMap();
        cells = new GameObject[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {        
                GameObject cell = Instantiate(cellPrefab, new Vector3(i, 0, j), new Quaternion(), transform);
                List<CellType> totalTypes = new List<CellType>();
                string[] info = cellInfo[i, j].Split(new char[] { '-' });
                foreach(string type in info)
                {
                    switch (type)
                    {
                        case "b":
                            totalTypes.Add(CellType.Block);
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
                cell.name = (i + " , " + j);
                cells[i, j] = cell;
            }
        }
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

