using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private GameObject player;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, 40, player.transform.position.z);
    }
}
