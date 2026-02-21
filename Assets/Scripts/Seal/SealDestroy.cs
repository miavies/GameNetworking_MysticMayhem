using TMPro;
using UnityEngine;

public class SealDestroy : MonoBehaviour
{
    [SerializeField] private GameObject sealDestroyed;
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshPro numDestroyedSeals;

    private void OnDestroy()
    {
        anim.SetTrigger("SealDestryedPrompt");

        int current = int.Parse(numDestroyedSeals.text);
        current++;
        numDestroyedSeals.text = current.ToString();
    }
}
