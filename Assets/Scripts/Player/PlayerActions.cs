using UnityEngine;
using Unity.Cinemachine;

public class PlayerActions : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject aimCam;

    [Header("Settings")]
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 100f;

    [SerializeField] private GameObject hitVFX;
    [SerializeField] GameObject crosshair;
    private float yRotation;
    private Camera unityMainCam;

    private Animator animator;



    void Start()
    {
        animator = GetComponent<Animator>();
        unityMainCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        crosshair = GameObject.Find("Crosshair");

        yRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && animator.GetBool("Aiming"))
        {
            animator.SetTrigger("Attack");
        }

        if (Input.GetMouseButtonDown(1))
        {
            animator.ResetTrigger("Attack");
            SnapPlayerToCamera();
            aimCam.SetActive(true);
            animator.SetBool("Aiming", true);
            crosshair.SetActive(true);
        }

        if (Input.GetMouseButtonUp(1))
        {
            aimCam.SetActive(false);
            animator.SetBool("Aiming", false);
            crosshair.SetActive(false);
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
    public void Fire()
    {
        Ray ray = new Ray(aimCam.transform.position, aimCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            NetworkHealth target = hit.collider.GetComponent<NetworkHealth>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            GameObject vfxInstance = Instantiate(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(vfxInstance, 2f);
        }
    }
}