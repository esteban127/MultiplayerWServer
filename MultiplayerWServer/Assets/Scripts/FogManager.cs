using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    MapManager map;
    float viewRange = 6.05f;
    float thickness = .3f;
    List<Cell> currentInVision;
    List<Cell> lastVisibles;

    public FogManager(MapManager _map)
    {
        map = _map;
        currentInVision = new List<Cell>();
        lastVisibles = new List<Cell>();
    }

    public void CalculateVision (List<Character> allieds, bool toggleFog)
    {
        if (allieds.Count > 0)
        {
            lastVisibles = currentInVision;
            int team = allieds[0].Team;
            currentInVision = new List<Cell>();
            List<Cell> newVisibles = new List<Cell>();
            foreach(Character character in allieds)
            {
                if (character.Alive)
                {
                    if (map.GetCellFromCoord(character.Pos).IsAValidBush())
                    {
                        List<Cell> bushSection = map.GetBushSectionByPos(character.Pos);
                        foreach (Cell cell in bushSection)
                        {
                            if (!currentInVision.Contains(cell))
                            {
                                cell.SetVisible(true, team, toggleFog);
                                currentInVision.Add(cell);
                            }
                        }
                    }
                    newVisibles = CalculateLineOfSight(character.Pos);
                    foreach (Cell cell in newVisibles)
                    {
                        if (!currentInVision.Contains(cell) && !cell.IsAValidBush())
                        {
                            cell.SetVisible(true, team, toggleFog);
                            currentInVision.Add(cell);
                        }
                    }
                }                
            }
            foreach(Cell cell in lastVisibles)
            {
                if (!currentInVision.Contains(cell))
                {
                    cell.SetVisible(false, team,toggleFog);
                }
            }
        }
    }

    private List<Cell> CalculateLineOfSight(Vector2 origin)
    {
        List<Cell> visibleCells = new List<Cell>();
        Cell[] currentCells;
        List<Cell> currentVisibles;
        Vector2 direction;
        Vector2 gap;
        for (int i = 0; i < 360; i+=5)
        {
            direction = new Vector2(Mathf.Cos(i) * viewRange, Mathf.Sin(i) * viewRange);
            gap = (new Vector2(direction.y, direction.x).normalized) * thickness / 2;
            currentCells = LinearSight(origin,direction,gap);
            currentVisibles = LinearSightObstruction(currentCells);
            foreach(Cell cell in currentVisibles)
            {
                if (!visibleCells.Contains(cell))
                {
                    visibleCells.Add(cell);
                }
            }
            currentCells = LinearSight(origin, direction, -gap);
            currentVisibles = LinearSightObstruction(currentCells);
            foreach (Cell cell in currentVisibles)
            {
                if (!visibleCells.Contains(cell))
                {
                    visibleCells.Add(cell);
                }
            }
        }
        return visibleCells;
    }

    private List<Cell> LinearSightObstruction(Cell[] affectedCells)
    {
        CellType blockedDirection = CellType.Block;
        List<Cell> validCells = new List<Cell>();
        for (int i = 0; i < affectedCells.Length; i++)
        {
            if (affectedCells[i] == null || affectedCells[i].cellType.Contains(CellType.Block))
                return validCells;

            if (affectedCells[i].cellType.Contains(blockedDirection))
                return validCells;

            if (affectedCells[i].cellType.Contains(CellType.Nblock))
                blockedDirection = CellType.Sblock;
            if (affectedCells[i].cellType.Contains(CellType.Sblock))
                blockedDirection = CellType.Nblock;
            if (affectedCells[i].cellType.Contains(CellType.Eblock))
                blockedDirection = CellType.Wblock;
            if (affectedCells[i].cellType.Contains(CellType.Wblock))
                blockedDirection = CellType.Eblock;

            validCells.Add(affectedCells[i]);            
        }
        return validCells;
    }
    public Cell[] LinearSight(Vector2 origin, Vector2 direcition, Vector2 gap)
    {
        List<Vector2> affectedsCoords = new List<Vector2>();
        Vector2 aim = direcition;
        int xDirection = aim.x < 0 ? -1 : 1;
        int yDirection = aim.y < 0 ? -1 : 1;
        float currentY = 0;
        float newY;
        for (float i = 0.5f; i < Mathf.Abs(aim.x); i++)
        {
            newY = CalculateY(aim, i * xDirection,gap);
            if (Mathf.Abs(newY) > Mathf.Abs(aim.y))
                newY = aim.y;
            for (int j = (int)Mathf.Abs(currentY); j <= (int)Mathf.Abs(newY); j++)
            {
                affectedsCoords.Add(new Vector2((int)i * xDirection, j * yDirection));
            }
            currentY = newY;
        }
        newY = aim.y;
        for (int j = (int)Mathf.Abs(currentY); j <= (int)Mathf.Abs(newY); j++)
        {
            if (!affectedsCoords.Contains(new Vector2((int)aim.x, j * yDirection)))
                affectedsCoords.Add(new Vector2((int)aim.x, j * yDirection));
        }        
        Cell[] cellsToReturn = new Cell[affectedsCoords.Count];
        for (int i = 0; i < affectedsCoords.Count; i++)
        {
            cellsToReturn[i] = map.GetCellFromCoord(affectedsCoords[i] + origin);
        }
        return cellsToReturn;
    }
    public float CalculateY(Vector2 slope, float x)
    {
        return slope.y / slope.x * (x) + (slope.y < 0 ? -0.5f : 0.5f);
    }
    public float CalculateY(Vector2 slope, float x, Vector2 gap)
    {
        return slope.y / slope.x * (x + gap.x) + gap.y + (slope.y < 0 ? -0.5f : 0.5f);
    }  

}
