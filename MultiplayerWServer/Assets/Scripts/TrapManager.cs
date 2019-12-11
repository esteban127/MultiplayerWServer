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

        foreach(Cell cell in affectedCells)
        {
            cell.AddTrap(trap);
            trapCells.Add(cell);
        }
        trap.affectedCells = trapCells;
    }

    public void NewTurn()
    {
        foreach(Trap trap in activeTraps)
        {
            trap.durationLeft--;
            if (trap.durationLeft <= 0)
                RemoveTrap(trap);
        }
    }

    public void RemoveTrap(Trap trap)
    {
        foreach(Cell cell in trap.affectedCells)
        {
            cell.RemoveTrapLocally(trap);
        }
        activeTraps.Remove(trap);
    }
}

public class Trap
{
    public Character caster;
    public List<Cell> affectedCells;
    public Ability ability;
    public int durationLeft;
}
