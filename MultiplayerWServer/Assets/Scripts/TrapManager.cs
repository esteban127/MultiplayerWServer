using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager
{
    List<Trap> activeTraps;

    public TrapManager()
    {
        activeTraps = new List<Trap>();
    }

    public void SetTrap(Character caster, List<Cell> affectedCells,Ability ability)
    {
        Trap trap = new Trap();
        trap.caster = caster;
        List<Cell> trapCells = new List<Cell>();
        trap.ability = ability;
        trap.durationLeft = (int)ability.duration;
        foreach(Cell cell in affectedCells)
        {
            cell.AddTrap(trap);
            trapCells.Add(cell);
        }
        ability.projectile.GetComponent<TrapModel>().SetTrap(caster.Pos, trapCells[trapCells.Count - 1].Pos);
        trap.affectedCells = trapCells;
        activeTraps.Add(trap);
    }

    public void NewTurn()
    {
        List<Trap> currentTraps = new List<Trap>();
        foreach(Trap trap in activeTraps)
        {
            trap.durationLeft--;
            if (trap.durationLeft <= 0)
            {
                RemoveTrap(trap);
            }
            else
            {
                currentTraps.Add(trap);
            }
        }
        activeTraps = currentTraps;
    }

    public void RemoveTrap(Trap trap)
    {
        foreach(Cell cell in trap.affectedCells)
        {
            cell.RemoveTrapLocally(trap);
        }
        trap.ability.projectile.GetComponent<TrapModel>().DisableTrap();        
    }
}

public class Trap
{
    public Character caster;
    public List<Cell> affectedCells;
    public Ability ability;
    public int durationLeft;
}
