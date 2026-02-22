using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class EnemyNetworkHealth : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public float currentHealth { get; set; }

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;
    public override void Spawned()
    {
        if (gameObject.CompareTag("Player"))
        {
            healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        }

        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
        }

        healthBar.maxValue = maxHealth;
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
        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void Die()
    {
        Runner.Despawn(Object);
    }
}