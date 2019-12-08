using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum statusType
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
    revealed,
    silenced,
    weakened
}

public class Character : MonoBehaviour
{
    //abilities

    [SerializeField] Ability ability0;
    [SerializeField] Ability ability1;
    [SerializeField] Ability ability2;
    [SerializeField] Ability ability3;

    [SerializeField] Ability ultimate;

    [SerializeField] Ability catalyzer0;
    [SerializeField] Ability catalyzer1;
    [SerializeField] Ability catalyzer2;


    private int team;
    public int Team { get { return team; } }
    int localTeam;
    public int LocalTeam { set { localTeam = value; } }
    string id;
    public string ID { get { return id; } set{ id = value; } }
    private Vector2 lastPos;
    public Vector2 LastPos { get { return lastPos; } }
    private Vector2 lastPosSeen;
    public Vector2 LastPosSeen { get { return lastPosSeen; } }
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
    private List<Status> currentStatus;
    public List<Status> CurrentStatus { get { return currentStatus; } }
    [SerializeField] GameObject model;
    [SerializeField] private int maxHealth;


    private void Update()
    {
        if (team != localTeam)
        {
            if (visible && !model.activeInHierarchy)
            {
                model.SetActive(true);
            }
            if (!visible && model.activeInHierarchy)
            {
                model.SetActive(false);
            }
        }
    }

    void ResetTurnValues()
    {
        alreadyMove = false;

        bool slowed = false;
        bool hasted = false;

        foreach (Status status in CurrentStatus)
        {
            if (status.type == statusType.slowed)
                slowed = true;
            if (status.type == statusType.hasted)
                hasted = true;
        }
        movScore = 48 + (slowed ? -20 : 0) + (hasted ? 20 : 0);
        movSpended = 0;
    }
    public void AddEnergy(int amount)
    {
        energy += amount;
        if (energy > 100)
        {
            energy = 100;
        }
    }

    public void NewTurn()
    {
        TickStatus();
        AddEnergy(5);
        ResetTurnValues();
    }
    public void TickStatus()
    {
        List<Status> activeStatus= new List<Status>();
        foreach(Status status in currentStatus)
        {
            status.duration -= 1;
            if (status.duration >0 )
            {
                activeStatus.Add(status);
            }
        }
        currentStatus = activeStatus;
    }
           
    public Ability GetAbility (int abilitySlot)
    {
        switch (abilitySlot)
        {
            case 0:
                return ability0;
            case 1:
                return ability1;
            case 2:
                return ability2;
            case 3:
                return ability3;
            case 4:
                return ultimate;
            case 5:
                return catalyzer0;
            case 6:
                return catalyzer1;
            case 7:
                return catalyzer2;
        }
        return null;
    }
    public void SetVisible(bool onSight)
    {
        bool invisible = false;
        bool revealed = false;

        foreach(Status status in CurrentStatus)
        {
            if (status.type == statusType.invisible)
                invisible = true;
            if (status.type == statusType.revealed)
                revealed = true;
        }
        if(onSight && !invisible|| revealed)
        {
            visible = true;
            lastPosSeen = Pos;
        }
        else
        {
            visible = false;
        }
    }
    public void ReceiveDamage(int damage, bool cover)
    {

    }
    public void ApplyStatus(Status status)
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
        currentStatus = new List<Status>();
        transform.position = new Vector3(pos.x,0,pos.y);
    }

    public void Move(Vector2 pos)
    {
        cellOwner = true;
        lastPos = currentPos;
        movScore -= Node.CalculateDistance(pos, currentPos);
        movSpended += Node.CalculateDistance(pos, currentPos);
        if (visible)
            lastPosSeen = pos;
        Debug.Log("Last pos seen: " + lastPosSeen.x + ", " + lastPosSeen.y);
        currentPos = pos;
        transform.position = new Vector3(pos.x, 0, pos.y);   
    } 
}
[System.Serializable]
public class Status
{
    public statusType type;
    public int duration;
}
