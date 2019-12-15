using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Invalid,
    Block,
    BuffEnergize,
    BuffHaste,
    BuffHeal,
    BuffMight,
    Empty,
    FogBush,    
    Unwalkable,
    Wcover,
    Ncover,
    Ecover,
    Scover,
    Wblock,
    Nblock,
    Eblock,
    Sblock,
}
public enum PickeableBuff
{
    None,
    Heal,
    semiHeal,
    Might,
    Energize,
    Haste,
}

public class Cell : MonoBehaviour
{
    public List<CellType> cellType;
    Material materialInstance;
    Material fogMaterial;
    Material buffMaterial;    
    bool beingClicked = false;
    [SerializeField] GameObject fog;
    [SerializeField] GameObject playerDeadHeal;
    [SerializeField] GameObject buff;
    [SerializeField] GameObject buffSpawner;
    [SerializeField] GameObject buffCoutner0;
    [SerializeField] GameObject buffCoutner1;
    [SerializeField] GameObject buffCoutner2;
    [SerializeField] GameObject buffCoutner3;
    [SerializeField] Color fogColor;
    [SerializeField] Color bushFogColor;
    [SerializeField] Color buffHealColor;
    [SerializeField] Color buffHasteColor;
    [SerializeField] Color buffEnergizeColor;
    [SerializeField] Color buffMightColor;
    [SerializeField] Color baseColor;
    [SerializeField] Color walkableColor;
    [SerializeField] Color sprintableColor;
    [SerializeField] Color currentCell;
    [SerializeField] Color abilityDamageAoE;
    [SerializeField] Color abilityCastRange;
    Color lastColor;
    bool visible_0 = false;
    public bool Visible_0 { get { return visible_0; } }
    bool visible_1 = false;
    public bool Visible_1 { get { return visible_1; } }
    bool bushActive = true;
    PickeableBuff currentBuff;
    PickeableBuff buffToSpawn;
    int teamOfDeadHeal;
    bool isABuffSpawner = false;
    int buffCharge = 0;
    List<Trap> traps;
    Vector2 pos;
    public Vector2 Pos { get { return pos; } set { pos = value; } }
    
    private void Awake()
    {
        materialInstance = gameObject.GetComponent<Renderer>().material;
        fogMaterial = fog.GetComponent<Renderer>().material;
        buffMaterial = buff.GetComponent<Renderer>().material;
        traps = new List<Trap>();
        SetBaseColor();
    }
    public void SetVisible(bool isVisible, int team, bool toggleFog)
    {
        if (toggleFog)
        {
            fog.SetActive(!isVisible);
        }
        if(team == 0)
        {
            visible_0 = isVisible;
        }
        else
        {
            visible_1 = isVisible;
        }
    }
    public void NewTurn()
    {
        SetBaseColor();
        if (isABuffSpawner && buffCharge < 4)
        {
            TickBuffSpawn();
        }
    }

    private void TickBuffSpawn()
    {
        buffCharge++;
        switch (buffCharge)
        {
            case 1:
                buffCoutner0.SetActive(true);
                break;
            case 2:
                buffCoutner1.SetActive(true);
                break;
            case 3:
                buffCoutner2.SetActive(true);
                break;
            case 4:
                buffCoutner3.SetActive(true);
                buff.SetActive(true);
                currentBuff = buffToSpawn;
                break;
        }
    }
    public bool IsAValidBush()
    {
        return cellType.Contains(CellType.FogBush) && bushActive;
    }
    public List<Trap> CheckTraps(int team)
    {
        List<Trap> enemyTraps = new List<Trap>();
        foreach(Trap trap in traps)
        {
            if(trap.caster.Team != team)
            {
                enemyTraps.Add(trap);
            }
        }
        return enemyTraps;
    }
    public void SpawnPlayerDeadHeal(int team,bool isEnemy)
    {
        currentBuff = PickeableBuff.semiHeal;
        teamOfDeadHeal = team;
        playerDeadHeal.SetActive(true);
        if (isEnemy)
        {
            playerDeadHeal.GetComponent<Renderer>().material.color = buffHealColor;
            
        }
        else
        {
            playerDeadHeal.GetComponent<Renderer>().material.color = fogColor;
        }
    }
    public PickeableBuff PickBuff(int team)
    {
        PickeableBuff buffToReturn = currentBuff;
        if(currentBuff!= PickeableBuff.None)
        {

            if (buff.activeInHierarchy)
            {
                buff.SetActive(false);
                buffCharge = 0;
                buffCoutner0.SetActive(false);
                buffCoutner1.SetActive(false);
                buffCoutner2.SetActive(false);
                buffCoutner3.SetActive(false);
            }            
          
            if(currentBuff == PickeableBuff.semiHeal)
            {
                if(teamOfDeadHeal == team)
                {
                    return PickeableBuff.None;
                }
                playerDeadHeal.SetActive(false);
            }
            currentBuff = PickeableBuff.None;
        }
        return buffToReturn; 
    }
    public void SetBuffSpawner(CellType buffType)
    {
        isABuffSpawner = true;
        buffSpawner.SetActive(true);
        buffCharge = 2;
        Color currentBuffColor = buffHealColor;
        switch (buffType)
        {
            case CellType.BuffEnergize:
                currentBuffColor = buffEnergizeColor;
                buffToSpawn = PickeableBuff.Energize;
                break;
            case CellType.BuffHeal:
                currentBuffColor = buffHealColor;
                buffToSpawn = PickeableBuff.Heal;
                break;
            case CellType.BuffHaste:
                currentBuffColor = buffHasteColor;
                buffToSpawn = PickeableBuff.Haste;
                break;
            case CellType.BuffMight:
                currentBuffColor = buffMightColor;
                buffToSpawn = PickeableBuff.Might;
                break;
        }
        buff.GetComponent<Renderer>().material.color = currentBuffColor;
        buffCoutner0.GetComponent<SpriteRenderer>().color = currentBuffColor;
        buffCoutner1.GetComponent<SpriteRenderer>().color = currentBuffColor;
        buffCoutner2.GetComponent<SpriteRenderer>().color = currentBuffColor;
        buffCoutner3.GetComponent<SpriteRenderer>().color = currentBuffColor;
        buffCoutner0.SetActive(true);
        buffCoutner1.SetActive(true);
    }
    public void AddTrap(Trap trap)
    {
        traps.Add(trap);
    }

    public void RemoveTrapLocally(Trap trap)
    {
        traps.Remove(trap);
    }

    public void HighLight(bool highlighted)
    {
        if (!beingClicked)
        {            
            Color color = materialInstance.color;
            color.a = highlighted ? 0.75f : 0.5f;
            materialInstance.color = color;
        }        
    }

    public void ClickedHighLight(bool clicked)
    {
        beingClicked = clicked;
        Color color = materialInstance.color;
        color.a = clicked ? 0.9f : 0.5f;
        materialInstance.color = color;
    }
    public void SetBaseColor()
    {
        materialInstance.color = baseColor;
    }
    public void SetCurrentCellColor()
    {
        materialInstance.color = currentCell;
    }
    public void SetWalkableColor()
    {
        materialInstance.color = walkableColor;
    }
    public void SetSprintableColor()
    {
        materialInstance.color = sprintableColor;
    }
    public void SetAbilityCastRange()
    {
        materialInstance.color = abilityCastRange;
    }
    public void SetAbilityDamageAoE()
    {
        materialInstance.color = abilityDamageAoE;        
    }    
    public void SetBushFogColor()
    {
        if(fogMaterial == null)
        {
            fogMaterial = fog.GetComponent<Renderer>().material;
        }
        fogMaterial.color = bushFogColor;
    }

}
public class Node
{
    public int h;
    public int g;
    public int f;
    bool walkable;
    public bool walked = false;
    public bool Walkable { get { return walkable; } }
    Vector2 pos;
    public Vector2 Pos { get { return pos; } }
    Node parent;
    public Node Parent { get { return parent; } }
    public Node(Vector2 coord, bool esCaminable)
    {
        pos = coord;
        this.walkable = esCaminable;
    }  

    public void CalculateF(Node from, Vector2 goal)
    {
        parent = from;
        h = CalculateDistance(pos, goal);
        g = CalculateDistance(pos, from.pos);
        g += from.g;
        f = h + g;
    }
    public void CalculateOnlyG(Node from)
    {
        parent = from;
        g = CalculateDistance(pos, from.pos);
        g += from.g;
    }



    public static int CalculateDistance(Vector2 from, Vector2 to)
    {
        int distance = 0;
        int x = (int)Mathf.Abs(to.x - from.x);
        int y = (int)Mathf.Abs(to.y - from.y);
        if (x <= y)
            distance += x * 14;
        else
            distance += y * 14;

        distance += Mathf.Abs(x - y) * 10;
        return distance;

    }
    public int CalculateDistance(Vector2 to)
    {
        int distance = 0;
        int x = (int)Mathf.Abs(to.x - pos.x);
        int y = (int)Mathf.Abs(to.y - pos.y);
        if (x <= y)
            distance += x * 14;
        else
            distance += y * 14;

        distance += Mathf.Abs(x - y) * 10;
        return distance;

    }


}