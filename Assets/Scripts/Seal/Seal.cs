using UnityEngine;

public class Seal : MonoBehaviour
{
    [SerializeField] private SealDestroy sealManager;

    private void OnDestroy()
    {
        if (sealManager != null)
        {
            sealManager.SealWasDestroyed();
        }
    }
}