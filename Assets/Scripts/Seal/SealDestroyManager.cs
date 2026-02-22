using Fusion;
using TMPro;
using UnityEngine;

public class SealDestroy : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI numDestroyedSeals;

    [Header("Effects")]
    [SerializeField] private GameObject sealDestroyedPromp;
    [SerializeField] private GameObject endGoal;
    private Animator anim;

    [Networked]
    public int numSeal { get; set; }

    public override void Spawned()
    {
        anim = sealDestroyedPromp.GetComponent<Animator>();
        UpdateSealUI();
        endGoal.SetActive(false);
    }
    public override void FixedUpdateNetwork()
    {
        UpdateSealUI();
        
    }

    public void SealWasDestroyed()
    {
        if (Object.HasStateAuthority)
        {
            numSeal++;
            RPC_PlayDestroyAnimation();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDestroyAnimation()
    {
        if (numSeal == 3)
        {
            numDestroyedSeals.text = "Destroyed ALL Seals";
            endGoal.SetActive(true);
        }

        if (anim != null)
            anim.SetTrigger("DestroySeal");
        UpdateSealUI();
    }

    

    private void UpdateSealUI()
    {
        if (numDestroyedSeals != null)
        {
            numDestroyedSeals.text = numSeal.ToString();
        }
    }
}