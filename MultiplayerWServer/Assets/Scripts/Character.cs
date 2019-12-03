using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum status
{
    might,
    hasted,
    energized,
    unstopable,
    invisible,
    healing,
    slowed,
    rooted,
    taunted,
    revealeed,
    silenced,
    weakened
}

public class Character : MonoBehaviour
{
    private int team;
    public int Team { get { return team; } }
    private Vector2 lastPos;
    public Vector2 LastPos { get { return lastPos; } }
    private Vector2 currentPos;
    public Vector2 Pos { get { return currentPos; } }
    private bool alreadyMove = false;
    public bool AlreadyMove { get { return alreadyMove; } set { alreadyMove = value; } }
    private bool cellOwner = true;
    public bool CellOwner { get { return cellOwner; } set { cellOwner = value; } }
    private bool visible = true;
    public bool Visible { get { return visible; } set { visible = value; } }
    private int energy = 0;
    private int health;
    private int shield = 0;
    private int movScore;
    public int MovScore { get { return movScore; } set{ movScore = value; } }
    public int movSpended = 0;
    private List<status> currentStatus;
    public List<status> CurrentStatus { get { return currentStatus; } }

    [SerializeField] private int maxHealth;

    void ResetTurnValues()
    {
        alreadyMove = false;
        movScore = 48 + (currentStatus.Contains(status.slowed) ? -20 : 0) + (currentStatus.Contains(status.hasted) ? 20 : 0);
        movSpended = 0;
    }
    public void NewTurn()
    {
        //buffs/debuffs duration --
        ResetTurnValues();
    }
    public void ReceiveDamage(int damage, bool cover)
    {

    }
    public void ApplyStatus(status status)
    {
        Debug.Log("gain status " + status.ToString());
    }
    public void Spawn(Vector2 pos, int _team)
    {
        movScore = 200; //turn 1
        movSpended = 0;
        team = _team;
        Spawn(pos);        
    }
    public void Spawn(Vector2 pos)
    {
        movSpended = 0;
        currentPos = pos;
        health = maxHealth;
        shield = 0;
        currentStatus = new List<status>();
        transform.position = new Vector3(pos.x,0,pos.y);
    }

    public void Move(Vector2 pos)
    {
        cellOwner = true;
        lastPos = currentPos;
        movScore -= Node.CalculateDistance(pos,currentPos);
        movSpended += Node.CalculateDistance(pos, currentPos);
        currentPos = pos;
        transform.position = new Vector3(pos.x, 0, pos.y);   
    } 
}
