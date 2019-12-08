using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum abilityType
{
    preparation,
    dash,
    fire
}

public enum abilityTags
{
    ultimate,
    catalyzer,
    selfBuff,
    explosiveProjectile,
    price,
    ignoreWalls,
    trap,
    energyPerEnemyHit,
    energyOnActivate
}

public class Ability : MonoBehaviour
{
    public string abilityName;
    public int cooldown;
    public int cost; // 0 free - 1 normal - 2 cannotMove
    public abilityType type;
    public aimType aim;
    public abilityTags[] tags;
    public Status[] statusToApply;
    public float range0;
    public float range1;
    public float thickness;
    public float duration;
    public int energyCost;
    public int energyProduced;
    public int damage0;
    public int damate1;
    public int damage2;
    public Sprite icon;
}
