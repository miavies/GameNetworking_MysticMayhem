using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    public float MoveSmoothTime = 0.1f;
    public float GravityStrength = 9.81f;

    [Header("Camera")]
    public Transform CameraTransform;

    private CharacterController controller;
    private Animator anim;

    private Vector3 currentVelocity;
    private Vector3 velocityDamp;

    private Vector3 gravityVelocity;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (CameraTransform == null && Camera.main != null)
            CameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (input.magnitude > 1f) input.Normalize();

        bool isMoving = input.sqrMagnitude > 0.001f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? RunSpeed : WalkSpeed;

        Vector3 camForward = CameraTransform.forward;
        Vector3 camRight = CameraTransform.right;


        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * input.z + camRight * input.x;

        bool isAiming = anim.GetBool("Aiming");
        if (isMoving && !isAiming)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                10f * Time.deltaTime
            );
        }

        currentVelocity = Vector3.SmoothDamp(currentVelocity, move * speed, ref velocityDamp, MoveSmoothTime);
        controller.Move(currentVelocity * Time.deltaTime);

        float animSpeed = isMoving ? (isRunning ? 1f : 0.5f) : 0f;
        anim.SetFloat("Speed", animSpeed);
    }

    void HandleGravity()
    {
        if (controller.isGrounded)
        {
            gravityVelocity.y = -2f;
        }
        else
        {
            gravityVelocity.y -= GravityStrength * Time.deltaTime;
        }

        controller.Move(gravityVelocity * Time.deltaTime);
    }
}
