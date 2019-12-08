using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum aimType
{
    linear,
    cone,
    cell,
    selfBuff,
    lineAoE
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

    public HitInfo PredictiveAim(Vector2 origin, Vector2 end, float range,float thickness, aimType type, int team, int dmg)
    {
        switch (type)
        {
            case aimType.linear:
                return LinearImpact(origin, end, range,thickness,team,dmg,true,true);
                
        }
        return new HitInfo();
    }
    public HitInfo CheckImpact(Vector2 origin, Vector2 end, float range, float thickness ,aimType type, int team, int dmg)
    {
        switch (type)
        {
            case aimType.linear:
                return LinearImpact(origin, end, range,thickness ,team, dmg,false,false);
                
        }
        return new HitInfo();
    }

    private HitInfo LinearImpact(Vector2 origin, Vector2 end, float range,float thickness ,int team, int dmg, bool onlyVisible, bool drawFloor)
    {
        HitInfo info = new HitInfo();
        Vector2 direction = (end - origin).normalized * range;
        Vector2 gap = (new Vector2(direction.y,direction.x).normalized) * thickness / 2;        
        Debug.DrawLine(new Vector3((origin - gap).x,1, (origin - gap).y), new Vector3((direction - gap+ origin).x, 1, (direction - gap +origin).y));
        Debug.DrawLine(new Vector3((origin).x, 1, (origin).y), new Vector3((direction + origin).x, 1, (direction + origin).y));
        Debug.DrawLine(new Vector3((origin + gap).x, 1, (origin + gap).y), new Vector3((direction + gap + origin).x, 1, (direction + gap + origin).y));
        Cell[] affectedCells_0 = LinearAim(origin, direction,-gap);
        Cell[] affectedCells_1 = LinearAim(origin, direction);
        Cell[] affectedCells_2 = LinearAim(origin, direction,gap);
        List<Cell> validCells_0 = CalculateLinearImpact(affectedCells_0, team, onlyVisible);
        List<Cell> validCells_1 = CalculateLinearImpact(affectedCells_1, team, onlyVisible);
        List<Cell> validCells_2 = CalculateLinearImpact(affectedCells_2, team, onlyVisible);
        float farestDistance;
        Cell lastCell = map.GetCellFromCoord(origin);
        Character posibleImpactedCharacter_1 = null;
        Character posibleImpactedCharacter_2 = null;
        Character posibleImpactedCharacter_0 = null;
        if (validCells_0.Count > 0)
        {
            posibleImpactedCharacter_0 = map.GetCharacterByPos(validCells_0[validCells_0.Count - 1].Pos, false);
        }        
        if (validCells_1.Count > 0)
        {
            posibleImpactedCharacter_1 = map.GetCharacterByPos(validCells_1[validCells_1.Count - 1].Pos, false);
        }        
        if (validCells_2.Count > 0)
        {
            posibleImpactedCharacter_2 = map.GetCharacterByPos(validCells_2[validCells_2.Count - 1].Pos, false);
        }
        if ((posibleImpactedCharacter_0 != null && posibleImpactedCharacter_0.Team != team)|| (posibleImpactedCharacter_1 != null && posibleImpactedCharacter_1.Team != team)|| (posibleImpactedCharacter_2 != null && posibleImpactedCharacter_2.Team != team))
        {
            float charDistance_0 = 100.0f;
            float charDistance_1 = 100.0f;
            float charDistance_2 = 100.0f;
            if ((posibleImpactedCharacter_0 != null && posibleImpactedCharacter_0.Team != team))
            {
                charDistance_0 = (posibleImpactedCharacter_0.Pos - origin).magnitude;
            }
            if ((posibleImpactedCharacter_1 != null && posibleImpactedCharacter_1.Team != team))
            {
                charDistance_1 = (posibleImpactedCharacter_1.Pos - origin).magnitude;
            }
            if ((posibleImpactedCharacter_2 != null && posibleImpactedCharacter_2.Team != team))
            {
                charDistance_2 = (posibleImpactedCharacter_2.Pos - origin).magnitude;
            }
            if (charDistance_0 <= charDistance_1 && charDistance_0 <= charDistance_2)
            {
                farestDistance = charDistance_0;
                lastCell = map.GetCellFromCoord(posibleImpactedCharacter_0.Pos);
            }
            else
            {
                if (charDistance_1 <= charDistance_2)
                {
                    farestDistance = charDistance_1;
                    lastCell = map.GetCellFromCoord(posibleImpactedCharacter_1.Pos);
                }
                else
                {
                    farestDistance = charDistance_2;
                    lastCell = map.GetCellFromCoord(posibleImpactedCharacter_2.Pos);
                }
            }
        }
        else
        {
            float distance_0 = 0;
            float distance_1 = 0;
            float distance_2 = 0;
            if (validCells_0.Count > 0)
                distance_0 = (validCells_0[validCells_0.Count - 1].Pos - origin).magnitude;
            if (validCells_1.Count > 0)
                distance_1 = (validCells_1[validCells_1.Count - 1].Pos - origin).magnitude;
            if (validCells_2.Count > 0)
                distance_2 = (validCells_2[validCells_2.Count - 1].Pos - origin).magnitude;

            if (distance_0 >= distance_1 && distance_0 >= distance_2)
            {
                farestDistance = distance_0;
                if (validCells_0.Count > 0)
                    lastCell = map.GetCellFromCoord(validCells_0[validCells_0.Count - 1].Pos);
            }
            else
            {
                if (distance_1 >= distance_2)
                {
                    farestDistance = distance_1;
                    if (validCells_1.Count > 0)
                        lastCell = map.GetCellFromCoord(validCells_1[validCells_1.Count - 1].Pos);
                }
                else
                {
                    farestDistance = distance_2;
                    if (validCells_2.Count > 0)
                        lastCell = map.GetCellFromCoord(validCells_2[validCells_2.Count - 1].Pos);
                }
            }
        }
        direction = (end - origin).normalized * (farestDistance+0.5f);
        affectedCells_0 = LinearAim(origin, direction, -gap);
        affectedCells_1 = LinearAim(origin, direction);
        affectedCells_2 = LinearAim(origin, direction, gap);
        validCells_0 = CalculateLinearImpact(affectedCells_0, team, onlyVisible);
        validCells_1 = CalculateLinearImpact(affectedCells_1, team, onlyVisible);
        validCells_2 = CalculateLinearImpact(affectedCells_2, team, onlyVisible);
        List<Cell> validCells = new List<Cell>();
        foreach(Cell cell in validCells_0)
        {
            if (!validCells.Contains(cell))
            {
                validCells.Add(cell);
            }
        }
        foreach (Cell cell in validCells_1)
        {
            if (!validCells.Contains(cell))
            {
                validCells.Add(cell);
            }
        }
        foreach (Cell cell in validCells_2)
        {
            if (!validCells.Contains(cell))
            {
                validCells.Add(cell);
            }
        }

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
        Character impactedCharacter = map.GetCharacterByPos(lastCell.Pos, false);
        if((impactedCharacter != null && impactedCharacter.Team != team))
        {
            info.target = impactedCharacter;
            info.damage = dmg;
            if((lastCell.Pos - origin).magnitude<=1)
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
    private List<Cell> CalculateLinearImpact(Cell[] affectedCells, int team, bool onlyVisible)
    {
        CellType blockedDirection = CellType.Invalid;
        List<Cell> validCells = new List<Cell>();
        for (int i = 0; i < affectedCells.Length; i++)
        {
            if (affectedCells[i] == null || affectedCells[i].cellType.Contains(CellType.Block))
                return validCells;

            if (blockedDirection != CellType.Invalid)
            {
                if (affectedCells[i].cellType.Contains(blockedDirection))
                    return validCells;
                else
                {
                    blockedDirection = CellType.Invalid;
                }
            }

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
    private List<Cell> CalculateLinearImpactWithoutColision(Cell[] affectedCells,int team, bool onlyVisible)
    {
        CellType blockedDirection = CellType.Invalid;
        List<Cell> validCells = new List<Cell>();
        bool invalidCell;
        for (int i = 0; i < affectedCells.Length; i++)
        {
            invalidCell = false;
            if (affectedCells[i] == null)
                return validCells;
            if (affectedCells[i].cellType.Contains(CellType.Block))
                invalidCell = true;

            if(blockedDirection != CellType.Invalid)
            {
                if (affectedCells[i].cellType.Contains(blockedDirection))
                    invalidCell = true;
                else
                {
                    blockedDirection = CellType.Invalid;
                }
            }            

            if (affectedCells[i].cellType.Contains(CellType.Nblock))
                blockedDirection = CellType.Sblock;
            if (affectedCells[i].cellType.Contains(CellType.Sblock))
                blockedDirection = CellType.Nblock;
            if (affectedCells[i].cellType.Contains(CellType.Eblock))
                blockedDirection = CellType.Wblock;
            if (affectedCells[i].cellType.Contains(CellType.Wblock))
                blockedDirection = CellType.Eblock;

            if(!invalidCell)
                validCells.Add(affectedCells[i]);
            if (map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible) != null && map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible).Team != team)
                return validCells;
        }
        return validCells;
    }

    public Cell[] LinearAim(Vector2 origin, Vector2 direcition, Vector2 gap)
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
                affectedsCoords.Add(new Vector2(( int)aim.x, j* yDirection));
        }
        Cell[] cellsToReturn = new Cell[affectedsCoords.Count];
        for (int i = 0; i < affectedsCoords.Count; i++)
        {
            cellsToReturn[i] = map.GetCellFromCoord(affectedsCoords[i]+origin);
        }
        if (cellsToReturn.Length > 11)
        {
            return cellsToReturn;
        }
        return cellsToReturn;
    }
    public float CalculateY(Vector2 slope, float x)
    {
        return slope.y / slope.x * (x) + (slope.y < 0 ? -0.5f : 0.5f);
    }
    public float CalculateY(Vector2 slope, float x, Vector2 gap)
    {
        return slope.y / slope.x * (x + gap.x) + gap.y +(slope.y<0? -0.5f:0.5f);
    }
    public Cell[] LinearAim(Vector2 origin, Vector2 direcition)
    {
        List<Vector2> affectedsCoords = new List<Vector2>();
        Vector2 aim = direcition;
        int xDirection = aim.x < 0 ? -1 : 1;
        int yDirection = aim.y < 0 ? -1 : 1;
        float currentY = 0;
        float newY;
        for (float i = 0.5f; i < Mathf.Abs(aim.x); i++)
        {
            newY = CalculateY(aim, i * xDirection);                        
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

}

public struct HitInfo
{
    public Character target;
    public int damage;
    public bool cover;
}
