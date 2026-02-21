using Fusion;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [Networked] public float currentHealth { get; set; }
    [SerializeField] private float maxHealth = 100f;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
        }
    }


    public void TakeDamage(float amount)
    {
        if (!HasStateAuthority) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Runner.Despawn(Object);
    }
}