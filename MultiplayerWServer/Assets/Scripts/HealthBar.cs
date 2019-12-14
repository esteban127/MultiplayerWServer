using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    [SerializeField] Slider energyBar;

    Transform owner;
    [SerializeField] float offset;

    public void SetOwner(Transform _owner)
    {
        owner = _owner;
    }

    public void ActualziateEnergyBar(int energy)
    {
        energyBar.value = (float)energy / 100;
        energyBar.GetComponentInChildren<Text>().text = energy.ToString();
    }
    public void ActualziateHealthBar(int health, int maxHealth)
    {
        healthBar.value = (float)health / (float)maxHealth;
        healthBar.GetComponentInChildren<Text>().text = health.ToString();
    }
    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(owner.position + Vector3.up * offset);
    }
}
