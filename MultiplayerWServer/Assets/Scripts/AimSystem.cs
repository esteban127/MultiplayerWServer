using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum aimType
{
    linear,
    cone,
    cell
}

public class AimSystem
{
    MapManager map;

    List<Cell> lastAimed;

    public AimSystem(MapManager _map)
    {
        map = _map;
        lastAimed = new List<Cell>();
    }

    public HitInfo PredictiveAim(Vector2 origin, Vector2 end, float range, aimType type, int team, int dmg)
    {
        switch (type)
        {
            case aimType.linear:
                return LinearImpact(origin, end, range, team,dmg,true,true);
                
        }
        return new HitInfo();
    }
    public HitInfo CheckImpact(Vector2 origin, Vector2 end, float range, aimType type, int team, int dmg)
    {
        switch (type)
        {
            case aimType.linear:
                return LinearImpact(origin, end, range, team, dmg,false,false);
                
        }
        return new HitInfo();
    }

    private HitInfo LinearImpact(Vector2 origin, Vector2 end, float range, int team, int dmg, bool onlyVisible, bool drawFloor)
    {
        HitInfo info = new HitInfo();
        Cell[] affectedCells = LinearAim(origin, end, range);
        List<Cell> validCells = CalculateLinearImpact(affectedCells, team, onlyVisible);
        if (drawFloor)
        {
            for (int i = 0; i < lastAimed.Count; i++)
            {
                lastAimed[i].SetBaseColor();
            }
            for (int i = 0; i < validCells.Count; i++)
            {
                validCells[i].SetAbilityDamageAoE();
            }
        }        
        lastAimed = validCells;
        Vector2 direction = end - origin ;
        Cell lastCell = validCells[validCells.Count - 1];
        Character impactedCharacter = map.GetCharacterByPos(lastCell.Pos, false);
        if((impactedCharacter != null && impactedCharacter.Team != team))
        {
            info.target = impactedCharacter;
            info.damage = dmg;
            if(direction.magnitude < 2)
            {
                info.cover = false; // mele hits ignore cover
                return info;
            }
            if (lastCell.cellType.Contains(CellType.Ecover))
            {
                if (Vector2.Angle(direction, new Vector2(0, -1)) < 30.0f)
                {
                    info.cover = true;
                    return info;
                }
                    
            }
            if (lastCell.cellType.Contains(CellType.Ncover))
            {
                if (Vector2.Angle(direction, new Vector2(1, 0)) < 30.0f)
                {
                    info.cover = true;
                    return info;
                }
            }
            if (lastCell.cellType.Contains(CellType.Wcover))
            {
                if (Vector2.Angle(direction, new Vector2(0, 1)) < 30.0f)
                {
                    info.cover = true;
                    return info;
                }
            }
            if (lastCell.cellType.Contains(CellType.Scover))
            {
                if (Vector2.Angle(direction, new Vector2(-1, 0)) < 30.0f)
                {
                    info.cover = true;
                    return info;
                }
            }
        }
        return info;
    }
   
    public void RestoreFloorColors()
    {
        for (int i = 0; i < lastAimed.Count; i++)
        {
            lastAimed[i].SetBaseColor();
        }
    }
    private List<Cell> CalculateLinearImpact(Cell[] affectedCells,int team, bool onlyVisible)
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
            if (map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible) != null && map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible).Team != team)
                return validCells;
        }
        return validCells;
    }

    public Cell[] LinearAim(Vector2 origin, Vector2 end,float range)
    {
        List<Vector2> affectedsCoords = new List<Vector2>();
        Vector2 Aim = (end - origin).normalized * range;
        int xDirection = Aim.x < 0 ? -1 : 1;
        int yDirection = Aim.y < 0 ? -1 : 1;
        float currentY = 0;
        float newY;
        for (float i = 0.5f; i < Mathf.Abs(Aim.x); i++)
        {
            newY = calculateY(Aim, i*xDirection);
            for (int j = (int)Mathf.Abs(currentY); j <= (int)Mathf.Abs(newY); j++)
            {
                affectedsCoords.Add(new Vector2((int)i*xDirection, j*yDirection));
            }
            currentY = newY;
        }
        newY = Aim.y;
        for (int j = (int)Mathf.Abs(currentY); j <= (int)Mathf.Abs(newY); j++)
        {
            if (!affectedsCoords.Contains(new Vector2((int)Aim.x, j * yDirection)))
                affectedsCoords.Add(new Vector2(( int)Aim.x, j* yDirection));
        }
        Cell[] cellsToReturn = new Cell[affectedsCoords.Count];
        for (int i = 0; i < affectedsCoords.Count; i++)
        {
            cellsToReturn[i] = map.GetCellFromCoord(affectedsCoords[i]+origin);
        }
        return cellsToReturn;
    }
    public float calculateY(Vector2 slope,float x)
    {
        return slope.y / slope.x * (x);
    }
}
public struct HitInfo
{
    public Character target;
    public int damage;
    public bool cover;
}
