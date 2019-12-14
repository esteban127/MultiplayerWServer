using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StatusType
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


    HealthBar healthBar;
    public HealthBar HealthBar { set { healthBar = value; } }

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
    public int health;
    private int realHealth;
    private int shield = 0;
    private int movScore;
    public int MovScore { get { return movScore; } set{ movScore = value; } }
    private int sprintScore = 38;
    public int SprintScore { get { return sprintScore; } }
    public int movSpended = 0;
    private List<Status> currentStatus;
    Dictionary<Ability, int> abilityCooldown;
    public List<Status> CurrentStatus { get { return currentStatus; } }
    [SerializeField] Material material;
    [SerializeField] Material inviMaterial;
    [SerializeField] GameObject model;
    [SerializeField] private int maxHealth;
    bool alive = true;
    public bool Alive { get { return alive; } set { alive = value; } }


    private void Update()
    {
        if (team != localTeam|| !alive)
        {
            if (visible && !model.activeInHierarchy)
            {
                model.SetActive(true);
                healthBar.gameObject.SetActive(true);
            }
            if (!visible && model.activeInHierarchy)
            {
                model.SetActive(false);
                healthBar.gameObject.SetActive(false);
            }
        }
    }
    private void Awake()
    {
        abilityCooldown = new Dictionary<Ability, int>();
        StartCooldowns();
    }
    public void NewTurn()
    {
        if (alive)
        {
            ActualizateEnergy(5);            
            ReplenishHealt(1);
            TickStatus();
            TickCooldowns();
            ResetTurnValues();
        }
    }
    private void TickCooldowns()
    {
        List<Ability> keys = new List<Ability>();
        foreach(Ability key in abilityCooldown.Keys)
        {
            keys.Add(key);
        }
        foreach(Ability key in keys)
        {
            if (abilityCooldown[key] > 0)
                abilityCooldown[key]--;
        }
    }
    void ResetTurnValues()
    {
        alreadyMove = false;

        bool slowed = false;
        bool hasted = false;

        foreach (Status status in CurrentStatus)
        {
            if (status.type == StatusType.slowed)
                slowed = true;
            if (status.type == StatusType.hasted)
                hasted = true;
        }
        movScore = 48 + (slowed ? -20 : 0) + (hasted ? 20 : 0);
        sprintScore = 38 + (hasted ? 20 : 0);
        movSpended = 0;
        realHealth = health;
    }
    public void RecalculateMovScore()
    {
        bool slowed = false;
        bool hasted = false;

        foreach (Status status in CurrentStatus)
        {
            if (status.type == StatusType.slowed)
                slowed = true;
            if (status.type == StatusType.hasted)
                hasted = true;
        }
        movScore = 48 + (slowed ? -20 : 0) + (hasted ? 20 : 0);
        sprintScore = 38 + (hasted ? 20 : 0);
        movScore -= movSpended;
    }
    public void ActualizateEnergy(int amount)
    {
        energy += amount;
        if (energy > 100)
        {
            energy = 100;
        }
        if(energy< 0)
        {
            energy = 0;
        }
        healthBar.ActualziateEnergyBar(energy);
    }
    public void AddEnergy(int amount)
    {
        ActualizateEnergy((int)(amount * EnergyMultiply()));
    }
    
    public void TickStatus()
    {
        List<Status> activeStatus= new List<Status>();
        foreach(Status status in currentStatus)
        {
            if(status.type == StatusType.healing)
            {
                ReplenishHealt(10);
            }
            status.duration -= 1;
            if (status.duration >0 )
            {
                activeStatus.Add(status);
            }
            else
            {
                if(status.type == StatusType.invisible)
                {
                    Debug.Log("Remove invi");
                    model.GetComponent<Renderer>().material = material;
                }
            }
        }
        currentStatus = activeStatus;
    }

    public Ability GetAbility(int abilitySlot)
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
    public Ability TryToGetAbility(int abilitySlot, int actionPoints)
    {
        Ability selectedAbility = null;
        switch (abilitySlot)
        {
            case 0:
                selectedAbility = ability0;
                break;
            case 1:
                selectedAbility = ability1;
                break;
            case 2:
                selectedAbility = ability2;
                break;
            case 3:
                selectedAbility = ability3;
                break;
            case 4:
                if (energy >= ultimate.energyCost && abilityCooldown[ultimate] <= 0 && actionPoints>= ultimate.cost)
                {
                    return ultimate;
                }
                else
                {
                    return null;
                }
            case 5:
                selectedAbility = catalyzer0;
                break;
            case 6:
                selectedAbility = catalyzer1;
                break;
            case 7:
                selectedAbility = catalyzer2;
                break;
        }
        if (selectedAbility != null && abilityCooldown[selectedAbility] == 0 && actionPoints>= selectedAbility.cost)
        {
            return selectedAbility;
        }
        else
        {
            return null;
        }
    }
    public void SetOnCooldown(int abilitySlot)
    {
        Ability key = null;
        switch (abilitySlot)
        {
            case 0:
                key = ability0;
                break;
            case 1:
                key = ability1;
                break;
            case 2:
                key = ability2;
                break;
            case 3:
                key = ability3;
                break;
            case 4:
                key = ultimate;
                energy -= ultimate.energyCost;
                break;
            case 5:
                key = catalyzer0;
                break;
            case 6:
                key = catalyzer1;
                break;
            case 7:
                key = catalyzer2;
                break;
        }
        abilityCooldown[key] = key.cooldown; 
    }
    public void SetOnCooldown(Ability ability)
    {        
        if(ability== ultimate)
        {
            energy -= ultimate.energyCost;
        }
        abilityCooldown[ability] = ability.cooldown;
    }
    public int GetCurrentCooldown(int abilitySlot)
    {
        Ability key = null;
        switch (abilitySlot)
        {
            case 0:
                key = ability0;
                break;
            case 1:
                key = ability1;
                break;
            case 2:
                key = ability2;
                break;
            case 3:
                key = ability3;
                break;
            case 4:
                key = ultimate;
                break;
            case 5:
                key = catalyzer0;
                break;
            case 6:
                key = catalyzer1;
                break;
            case 7:
                key = catalyzer2;
                break;
        }
        return abilityCooldown[key];
    }

    private void StartCooldowns()
    {
        abilityCooldown.Add(ability0, 0);
        abilityCooldown.Add(ability1, 0);
        abilityCooldown.Add(ability2, 0);
        abilityCooldown.Add(ability3, 0);
        abilityCooldown.Add(ultimate, 0);
        abilityCooldown.Add(catalyzer0, 0);
        abilityCooldown.Add(catalyzer1, 0);
        abilityCooldown.Add(catalyzer2, 0);
    }
    public void SetVisible(bool onSight)
    {
        bool invisible = false;
        bool revealed = false;

        foreach(Status status in CurrentStatus)
        {
            if (status.type == StatusType.invisible)
                invisible = true;
            if (status.type == StatusType.revealed)
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

    public bool CheckItsAlive()
    {
        if (realHealth <= 0)
        {
            Die();
            return false;
        }
        return true;
    }

    private void Die()
    {
        alive = false;
        healthBar.gameObject.SetActive(false);
        transform.position = new Vector3(-100, 100, -100);
    }

    public float DamageMultiply()
    {
        bool might = false;
        bool weak = false;

        foreach (Status status in CurrentStatus)
        {
            if (status.type == StatusType.might)
                might = true;
            if (status.type == StatusType.weakened)
                weak = true;
        }
        return 1 + (might ? 0.25f : 0) + (weak ? -0.25f :  0  );
    }
    public float EnergyMultiply()
    {
        bool energized = false;        

        foreach (Status status in CurrentStatus)
        {
            if (status.type == StatusType.energized)
                energized = true;            
        }
        return 1 + (energized ? 0.5f : 0);
    }

    public void ReceiveDamage(int damage, bool cover)
    {
        int dmg = (int)(damage * (cover ? 0.5f : 1));
        Debug.Log(gameObject.name + " Recive " + dmg + "Points of damage!");
        if (shield > 0)
        {
            if(shield> dmg)
            {
                shield -= dmg;
            }
            else
            {
                dmg -= shield;
                shield = 0;
                realHealth -= dmg;
                health = realHealth;
            }  
        }
        else
        {

            realHealth -= dmg;
            health = realHealth;
        }

        healthBar.ActualziateHealthBar(health, maxHealth);
    }
    public void ReplenishHealt(int ammount)
    {
        realHealth += ammount;
        health += ammount;
        if(health> maxHealth)
        {
            health = maxHealth;
        }

        healthBar.ActualziateHealthBar(health, maxHealth);
    }
    public void ApplyStatus(Status status)
    {
        bool statusExist = false;
        if(status.type == StatusType.healing)
        {
            ReplenishHealt(10);
        }        
        foreach (Status activeStatus in currentStatus)
        {
            if(activeStatus.type == status.type)
            {
                statusExist = true;
                if(status.duration> activeStatus.duration)
                {
                    activeStatus.duration = status.duration;
                }
            }
        }
        if (!statusExist && status.duration>0)
        {
            currentStatus.Add(status);
            if (status.type == StatusType.invisible)
            {
                model.GetComponent<Renderer>().material = inviMaterial;
            }
            if (status.type == StatusType.hasted || status.type == StatusType.slowed)
            {
                RecalculateMovScore();
            }
        }
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
        realHealth = health;
        ActualizateEnergy(0);
        healthBar.ActualziateHealthBar(health, maxHealth);
        shield = 0;
        currentStatus = new List<Status>();        
        model.GetComponent<Renderer>().material = material;
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
        currentPos = pos;
        transform.position = new Vector3(pos.x, 0, pos.y);   
    } 
}
[System.Serializable]
public class Status
{    
    public StatusType type;
    public int duration;
    public Status(StatusType _type, int _duration)
    {
        type = _type;
        duration = _duration;
    }
    public Status()
    {       
    }
}
