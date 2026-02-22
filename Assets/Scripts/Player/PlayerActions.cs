using Fusion;
using UnityEngine;

public class PlayerActions : NetworkBehaviour
{
    [Header("Cameras")]
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject aimCam;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 100f;

    [Header("Effects")]
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject crosshair;

    private Animator animator;

    private float yRotation;
    private bool isAiming;

    public override void Spawned()
    {
        animator = GetComponent<Animator>();

        mainCam.SetActive(Object.HasInputAuthority);
        aimCam.SetActive(false);
        crosshair.SetActive(false);

        yRotation = transform.eulerAngles.y;

        if (Object.HasInputAuthority)
            Cursor.lockState = CursorLockMode.Locked;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data))
            return;

        HandleAiming(data);
    }

    private void HandleAiming(NetworkInputData data)
    {
        if (data.AimPressed && !isAiming)
        {
            SnapPlayerToCamera(data.CameraForward);
            isAiming = true;
        }

        if (data.AimReleased)
        {
            isAiming = false;
        }

        if (isAiming)
        {
            yRotation += data.MouseDeltaX * mouseSensitivity;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        if (data.FirePressed && isAiming)
        {
            Fire(data.CameraPosition, data.CameraForward);
        }
    }

    private void SnapPlayerToCamera(Vector3 camForward)
    {
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(camForward);
            yRotation = transform.eulerAngles.y;
        }
    }

    private void Fire(Vector3 origin, Vector3 direction)
    {
        if (!Object.HasInputAuthority) return;

        RPC_Fire(origin, direction);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_Fire(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            NetworkHealth target = hit.collider.GetComponent<NetworkHealth>();

            if (target != null && target.CompareTag("Enemy"))
                target.TakeDamage(damage);

            if (hitVFX != null)
            {
                var vfx = Instantiate(hitVFX, hit.point,
                    Quaternion.LookRotation(hit.normal));

                Destroy(vfx, 2f);
            }
        }
    }

    void Update()
    {
        if (!Object.HasInputAuthority) return;

        aimCam.SetActive(isAiming);
        crosshair.SetActive(isAiming);

        animator.SetBool("Aiming", isAiming);
    }
}