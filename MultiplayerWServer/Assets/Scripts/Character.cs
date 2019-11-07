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
    private Vector2 currentPos;
    private int energy = 0;
    private int health;
    private int shield = 0;
    private int movScore;
    private List<status> currentStatus;

    [SerializeField] private int maxHealth;

    public void ResetTurnValues()
    {
        movScore = 48 + (currentStatus.Contains(status.slowed) ? -20 : 0) + (currentStatus.Contains(status.hasted) ? 20 : 0);        
    }
    public void Spawn(Vector2 pos, int _team)
    {
        team = _team;
        Spawn(pos);        
    }
    public void Spawn(Vector2 pos)
    {
        currentPos = pos;
        health = maxHealth;
        shield = 0;
        currentStatus = new List<status>();
        transform.position = new Vector3(pos.x,0,pos.y);
    }

}
