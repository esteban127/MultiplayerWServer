using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AimType
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
    TrapManager trapMan;

    List<Cell> lastAimed;

    public AimSystem(MapManager _map, TrapManager _trapM)
    {
        map = _map;
        trapMan = _trapM;
        lastAimed = new List<Cell>();
    }

    public List<HitInfo>[] PredictiveAim(Vector2 origin, Vector2 end,Character caster, Ability ability)
    {
        int team = caster.Team;
        List<HitInfo>[] hits = new List<HitInfo>[3];
        for (int i = 0; i < hits.Length; i++)
        {
            hits[i] = new List<HitInfo>();
        }
        switch (ability.aim)
        {
            case AimType.linear:

                if (ability.tags.Contains(AbilityTags.trap))
                {
                    hits[0].Add(LinearTrap(origin, end,caster, ability,true,false));
                }
                else
                {
                    hits[0].Add(LinearImpact(origin, end, ability.range0, ability.thickness, team, true, true));

                    if (ability.tags.Contains(AbilityTags.explosiveProjectile))
                    {
                        if (hits[0][0].target != null)
                        {
                            hits[1] = ExplosionImpact(hits[0][0].impactPos, hits[0][0].target, ability.range1, team, true, true);
                        }
                    }
                }                                
                break;
            case AimType.cell:
                if (ability.tags.Contains(AbilityTags.noDamage))
                {
                    hits[0].Add(CellCast(origin, end, ability.range0, true, ability.tags.Contains(AbilityTags.ignoreWalls)));
                }

                break;
            case AimType.lineAoE:
                hits[0] = LinearPirce(origin, end, ability.range0, ability.thickness, team, true, true, ability.tags.Contains(AbilityTags.ignoreWalls));
            break;
        }
        return hits;
    }
    

    public List<HitInfo>[] CheckImpact(Vector2 origin, Vector2 end, Character caster, Ability ability)
    {
        int team = caster.Team;
        List<HitInfo>[] hits = new List<HitInfo>[3];
        for (int i = 0; i < hits.Length; i++)
        {
            hits[i] = new List<HitInfo>();
        }
        switch (ability.aim)
        {
            case AimType.linear:
                if (ability.tags.Contains(AbilityTags.trap))
                {
                    hits[0].Add(LinearTrap(origin, end, caster, ability, false, true));
                }
                else
                {
                    hits[0].Add(LinearImpact(origin, end, ability.range0, ability.thickness, team, false, false));

                    if (ability.tags.Contains(AbilityTags.explosiveProjectile))
                    {
                        if (hits[0][0].target != null)
                        {
                            hits[1] = ExplosionImpact(hits[0][0].impactPos, hits[0][0].target, ability.range1, team, false, false);
                        }
                    }
                }
                break;

            case AimType.cell:
                if (ability.tags.Contains(AbilityTags.noDamage))
                {
                    hits[0].Add(CellCast(origin, end, ability.range0, false, ability.tags.Contains(AbilityTags.ignoreWalls)));
                }
                break;

            case AimType.lineAoE:
                hits[0] = LinearPirce(origin, end, ability.range0, ability.thickness, team, false, false, ability.tags.Contains(AbilityTags.ignoreWalls));
                break;
        }
        return hits;
    }
    private HitInfo CellCast(Vector2 origin, Vector2 target, float range, bool drawFloor,bool ignoreWalls)
    {
        HitInfo info = new HitInfo();
        List<Node> avilableNodes = map.GetAllNodesInARange(map.GetNodeFromACoord(origin), (int)range);
        Node aimingNode = map.GetNodeFromACoord(target + new Vector2(0.5f,0.5f));
        if (!ignoreWalls)
        {
            //should check with calculateLinearImpact
        }
        if (drawFloor)
        {
            lastAimed = new List<Cell>();
            Cell currentCell;
            foreach (Node node in avilableNodes)
            {
                currentCell = map.GetCellFromNode(node);
                lastAimed.Add(currentCell);
                if(node == aimingNode)
                {
                    currentCell.SetAbilityDamageAoE();
                }
                else
                {
                    currentCell.SetAbilityCastRange();
                }
            }
        }
        if (avilableNodes.Contains(aimingNode))
        {
            info.impactPos = target + new Vector2(0.5f, 0.5f);
        }
        else
        {
            info.impactPos = new Vector2(-1, -1);
        }
        return info;
    }
    private HitInfo LinearTrap(Vector2 origin, Vector2 end,Character caster, Ability ability, bool drawFloor,bool setTrap)
    {
        HitInfo info = new HitInfo();
        Vector2 direction = (end - origin).normalized * ability.range0;
        float thickness = ability.thickness;
        Cell[] affectedCells_1 = LinearAim(origin, direction);
        List<Cell> validCells_1 = CalculateLinearImpact(affectedCells_1);
        List<Cell> validCells = new List<Cell>();
        if (thickness > 0)
        {
            Vector2 gap = (new Vector2(direction.y, direction.x).normalized) * thickness / 2;

            Cell[] affectedCells_0 = LinearAim(origin, direction, -gap);
            Cell[] affectedCells_2 = LinearAim(origin, direction, gap);
            List<Cell> validCells_0 = CalculateLinearImpact(affectedCells_0);
            List<Cell> validCells_2 = CalculateLinearImpact(affectedCells_2);

            foreach (Cell cell in validCells_0)
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
        }
        else
        {
            validCells = validCells_1;
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

        if (setTrap)
        {
            trapMan.SetTrap(caster, validCells, ability);
        }

        return info;
    }

    private List<HitInfo> LinearPirce(Vector2 origin, Vector2 end, float range, float thickness, int team, bool onlyVisible, bool drawFloor, bool ignoreWalls)
    {
        List<HitInfo> info = new List<HitInfo>();
        Vector2 direction = (end - origin).normalized * range;

        Cell[] affectedCells_1 = LinearAim(origin, direction);
        List<Cell> validCells_1 = CalculateLinearImpactWithoutColision(affectedCells_1, team, onlyVisible, true);
        List<Cell> validCells = new List<Cell>();
        if (thickness > 0)
        {
            Vector2 gap = (new Vector2(direction.y, direction.x).normalized) * thickness / 2;
            Cell[] affectedCells_0 = LinearAim(origin, direction, -gap);
            Cell[] affectedCells_2 = LinearAim(origin, direction, gap);
            List<Cell> validCells_0 = CalculateLinearImpactWithoutColision(affectedCells_0, team, onlyVisible, true);
            List<Cell> validCells_2 = CalculateLinearImpactWithoutColision(affectedCells_2, team, onlyVisible, true);

            foreach (Cell cell in validCells_0)
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
        }
        else
        {
            validCells = validCells_1;
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

        HitInfo currentHit = new HitInfo();
        Character currentCharacter;
        Vector2 currentDirection;
        foreach (Cell cell in validCells)
        {
            currentCharacter = map.GetCharacterByPos(cell.Pos, onlyVisible);
            if (currentCharacter != null && currentCharacter.Team != team)
            {
                currentHit = new HitInfo();
                currentHit.target = currentCharacter;
                currentDirection = cell.Pos - origin;
                if ((currentDirection).magnitude <= 1)
                {
                    currentHit.cover = false; // mele hits ignore cover                    
                }
                else
                {
                    if (cell.cellType.Contains(CellType.Ecover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(0, -1)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }

                    }
                    if (cell.cellType.Contains(CellType.Ncover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(1, 0)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                    if (cell.cellType.Contains(CellType.Wcover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(0, 1)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                    if (cell.cellType.Contains(CellType.Scover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(-1, 0)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                }
                info.Add(currentHit);
            }
        }
        return info;
    }   

    private HitInfo LinearImpact(Vector2 origin, Vector2 end, float range,float thickness ,int team,bool onlyVisible, bool drawFloor)
    {
        HitInfo info = new HitInfo();
        Vector2 direction = (end - origin).normalized * range;
        info.impactPos = direction + origin;
        Vector2 gap = (new Vector2(direction.y,direction.x).normalized) * thickness / 2;        
        Debug.DrawLine(new Vector3((origin - gap).x,1, (origin - gap).y), new Vector3((direction - gap+ origin).x, 1, (direction - gap +origin).y));
        Debug.DrawLine(new Vector3((origin).x, 1, (origin).y), new Vector3((direction + origin).x, 1, (direction + origin).y));
        Debug.DrawLine(new Vector3((origin + gap).x, 1, (origin + gap).y), new Vector3((direction + gap + origin).x, 1, (direction + gap + origin).y));


        Cell[] affectedCells_1 = LinearAim(origin, direction);
        List<Cell> validCells_1 = CalculateLinearImpact(affectedCells_1, team, onlyVisible);
        Cell lastCell = map.GetCellFromCoord(origin);
        List<Cell> validCells = new List<Cell>();

        if (thickness > 0)
        {
            Cell[] affectedCells_0 = LinearAim(origin, direction, -gap);
            Cell[] affectedCells_2 = LinearAim(origin, direction, gap);
            List<Cell> validCells_0 = CalculateLinearImpact(affectedCells_0, team, onlyVisible);
            List<Cell> validCells_2 = CalculateLinearImpact(affectedCells_2, team, onlyVisible);
            float farestDistance;
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
            if ((posibleImpactedCharacter_0 != null && posibleImpactedCharacter_0.Team != team) || (posibleImpactedCharacter_1 != null && posibleImpactedCharacter_1.Team != team) || (posibleImpactedCharacter_2 != null && posibleImpactedCharacter_2.Team != team))
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
            direction = (end - origin).normalized * (farestDistance + 0.5f);
            affectedCells_0 = LinearAim(origin, direction, -gap);
            affectedCells_1 = LinearAim(origin, direction);
            affectedCells_2 = LinearAim(origin, direction, gap);
            validCells_0 = CalculateLinearImpact(affectedCells_0, team, onlyVisible);
            validCells_1 = CalculateLinearImpact(affectedCells_1, team, onlyVisible);
            validCells_2 = CalculateLinearImpact(affectedCells_2, team, onlyVisible);            
            foreach (Cell cell in validCells_0)
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
        }
        else
        {
            validCells = validCells_1;
            lastCell = validCells[validCells.Count - 1];
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
            info.impactPos = (end - origin).normalized * ((impactedCharacter.Pos - origin).magnitude) + origin;
            if ((lastCell.Pos - origin).magnitude<=1)
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

    private List<HitInfo> ExplosionImpact(Vector2 impactPos, Character impactedCharacter, float range, int team, bool onlyVisible, bool drawFloor)
    {
        List<HitInfo> impact = new List<HitInfo>();

        List<Cell> totalCells = new List<Cell>();
        Cell[] affectedCells;
        List<Cell> currentAffecteds;
        Vector2 direction;
        for (int i = 0; i < 360; i+=5)
        {
            direction = new Vector2(Mathf.Cos(i) * range, Mathf.Sin(i) * range);
            Debug.DrawLine(new Vector3(impactPos.x, 0.5f, impactPos.y), new Vector3(direction.x + impactPos.x, 0.5f, direction.y + impactPos.y));
            affectedCells = LinearAim(impactPos, direction);
            currentAffecteds = CalculateLinearImpact(affectedCells);
            if(i%90 == 0)
            {
                foreach(Cell cell in affectedCells)
                {
                    Debug.DrawLine(new Vector3(cell.Pos.x, 0, cell.Pos.y), new Vector3(cell.Pos.x, 3, cell.Pos.y), Color.blue);
                }
                foreach (Cell cell in currentAffecteds)
                {
                    Debug.DrawLine(new Vector3(cell.Pos.x, 0, cell.Pos.y), new Vector3(cell.Pos.x, 3, cell.Pos.y), Color.red);
                }
            }

            foreach (Cell cell in currentAffecteds)
            {
                if (!totalCells.Contains(cell))
                {
                    totalCells.Add(cell);
                }
            }            
        }
        if (drawFloor)
        {
            foreach (Cell cell in totalCells)
            {
                if (!lastAimed.Contains(cell))
                {
                    lastAimed.Add(cell);
                    cell.SetAbilityDamageAoE();
                }
            }
        }

        Character currentCharacter;
        HitInfo currentHit;
        Vector2 currentDirection;
        foreach (Cell cell in totalCells)
        {
            currentCharacter = map.GetCharacterByPos(cell.Pos, onlyVisible);
            if (currentCharacter != null && currentCharacter.Team != team && currentCharacter != impactedCharacter)
            {
                currentHit = new HitInfo();
                currentHit.target = currentCharacter;
                currentDirection = cell.Pos - impactPos;
                if ((currentDirection).magnitude <= 1)
                {
                    currentHit.cover = false; // mele hits ignore cover                    
                }
                else
                {
                    if (cell.cellType.Contains(CellType.Ecover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(0, -1)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }

                    }
                    if (cell.cellType.Contains(CellType.Ncover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(1, 0)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                    if (cell.cellType.Contains(CellType.Wcover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(0, 1)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                    if (cell.cellType.Contains(CellType.Scover))
                    {
                        if (Vector2.Angle(currentDirection, new Vector2(-1, 0)) < 30.0f)
                        {
                            currentHit.cover = true;
                        }
                    }
                }
                impact.Add(currentHit);
            }
        }

        return impact;
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
    private List<Cell> CalculateLinearImpact(Cell[] affectedCells)
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
        }
        return validCells;
    }
    private List<Cell> CalculateLinearImpactWithoutColision(Cell[] affectedCells,int team, bool onlyVisible, bool pirce)
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
            if (!pirce && map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible) != null && map.GetCharacterByPos(affectedCells[i].Pos, onlyVisible).Team != team)
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
        if (!affectedsCoords.Contains(new Vector2(aim.x + gap.x, newY +gap.y )))
            affectedsCoords.Add(new Vector2(aim.x + gap.x, newY +gap.y));
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
        if (!affectedsCoords.Contains(new Vector2((int)aim.x, newY * yDirection)))
            affectedsCoords.Add(new Vector2((int)aim.x, newY));
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
    public Vector2 impactPos;
    public bool cover;
}
