using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth = 50f;
    public float armor = 10f; // New armor attribute
    private float currentHealth;

    public float respawnTime = 2f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // Apply damage reduction based on armor
        float effectiveDamage = Mathf.Max(damage - armor, 0);
        currentHealth -= effectiveDamage;
        Debug.Log("Enemy took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetArmor()
    {
        return armor;
    }

    private void Die()
    {
        Debug.Log("Enemy died. Respawning in " + respawnTime + " seconds.");
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnTime);
    }

    private void Respawn()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        Debug.Log("Enemy respawned.");
    }
}


