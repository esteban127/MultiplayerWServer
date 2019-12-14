using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarsManager : MonoBehaviour
{
    [SerializeField] GameObject healthBarPrefab;
    public HealthBar GenerateHealthBar(Transform owner)
    {
        GameObject bar = Instantiate(healthBarPrefab, this.transform);
        bar.GetComponent<HealthBar>().SetOwner(owner);
        return bar.GetComponent<HealthBar>();
    }
}
