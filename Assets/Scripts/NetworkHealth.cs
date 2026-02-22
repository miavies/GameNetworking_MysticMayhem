using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkHealth : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public float currentHealth { get; set; }

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
            healthBar.maxValue = maxHealth;
        }

        // Server initializes health
        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthBar();
    }


    public void TakeDamage(float amount)
    {
        if (!HasStateAuthority) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void OnHealthChanged()
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!Object.HasInputAuthority) return;

        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void Die()
    {
        Runner.Despawn(Object);
    }
}