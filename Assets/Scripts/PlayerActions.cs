using UnityEngine;
using Unity.Cinemachine;

public class PlayerActions : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject aimCam;

    [Header("Settings")]
    [SerializeField] float mouseSensitivity = 2f;

    private float yRotation;
    private Camera unityMainCam;

    void Start()
    {
        unityMainCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;

        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SnapPlayerToCamera();
            aimCam.SetActive(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            aimCam.SetActive(false);
        }

        if (aimCam.activeSelf)
        {
            RotatePlayerWithMouse();
        }
    }

    void SnapPlayerToCamera()
    {
        Vector3 cameraForward = unityMainCam.transform.forward;
        cameraForward.y = 0;
        if (cameraForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(cameraForward);
            yRotation = transform.eulerAngles.y;
        }
    }

    void RotatePlayerWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}