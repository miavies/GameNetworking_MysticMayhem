using UnityEngine;

public class EndGoal : MonoBehaviour
{
    [SerializeField] private GameObject endPrompt;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            endPrompt.SetActive(true);
        }
    }
}
