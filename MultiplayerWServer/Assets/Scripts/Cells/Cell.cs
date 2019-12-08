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

public class Cell : MonoBehaviour
{
    public List<CellType> cellType;
    Material materialInstance;
    Material fogMateria;
    Material buffMaterial;    
    bool beingClicked = false;
    [SerializeField] GameObject fog;
    [SerializeField] GameObject buff;
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
    Vector2 pos;
    public Vector2 Pos { get { return pos; } set { pos = value; } }
    //List<Traps> traps
    private void Awake()
    {
        materialInstance = gameObject.GetComponent<Renderer>().material;
        fogMateria = fog.GetComponent<Renderer>().material;
        buffMaterial = buff.GetComponent<Renderer>().material;
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
    public bool IsAValidBush()
    {
        return cellType.Contains(CellType.FogBush) && bushActive;
    }
    public void CheckTraps(int team)
    {
        //shouldReturnTraps
    }
    public statusType CheckBuffs(int team)
    {
        //shouldReturnBuffs
        return 0; 
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

    public void NewTurn()
    {
        SetBaseColor();
        //trapduruation--
        //buffSpawn
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
        fogMateria.color = bushFogColor;
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