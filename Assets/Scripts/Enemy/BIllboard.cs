using UnityEngine;

public class BIllboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void LateUpdate()
    {
        transform.forward = mainCamera.transform.forward;
    }
}
