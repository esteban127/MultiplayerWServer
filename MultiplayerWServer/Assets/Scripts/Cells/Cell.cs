using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Block,
    BuffEnergize,
    BuffHaste,
    BuffHeal,
    BuffMight,
    Empty,
    Fog,    
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
    bool beingClicked = false;
    [SerializeField] Color baseColor;
    [SerializeField] Color walkableColor;
    [SerializeField] Color sprintableColor;

    private void Awake()
    {
        materialInstance = gameObject.GetComponent<Renderer>().material;
        SetBaseColor();
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
    public void SetWalkableColor()
    {
        materialInstance.color = walkableColor;
    }
    public void SetSprintableColor()
    {
        materialInstance.color = sprintableColor;
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



    int CalculateDistance(Vector2 from, Vector2 to)
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


}