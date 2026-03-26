using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyNetworkHealth : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public float currentHealth { get; set; }

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;

    [SerializeField] private WebRequest web;
    public override void Spawned()
    {
        web = GameObject.Find("Managers").GetComponentInChildren<WebRequest>();

        if (HasStateAuthority)
        {
            currentHealth = maxHealth;
        }

        healthBar.maxValue = maxHealth;
        UpdateHealthBar();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_OnEnemyKilled(PlayerRef killer)
    {
        if (Runner.LocalPlayer == killer)
        {
            NetworkObject playerObj = Runner.GetPlayerObject(killer);

            Debug.Log("RPC Found: " + playerObj);
            if (playerObj != null)
            {
                PlayerActions pActions = playerObj.GetComponent<PlayerActions>();
                Debug.Log("PlayerActions: " + pActions);
                if (pActions != null)
                {
                    pActions.score += 1;
                    Debug.Log("Score: " + pActions.score);
                    web.StartUpdateKills(pActions.score);
                }
            }
        }
    }

    public void TakeDamage(float amount, PlayerRef attacker)
    {
        if (!HasStateAuthority) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die(attacker);
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

    void Die(PlayerRef killer)
    {
        Debug.Log("Enemy died. Killer: " + killer);

        RPC_OnEnemyKilled(killer);

        Debug.Log("RPC Fired ");

        StartCoroutine(DespawnDelay());
    }

    IEnumerator DespawnDelay()
    {
        yield return new WaitForSeconds(0.2f);
        Runner.Despawn(Object);
    }
}