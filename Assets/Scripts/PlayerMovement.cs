using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float MoveSmoothTime;
    public float GravityStrength;
    public float WalkSpeed;
    public float RunSpeed;

    private CharacterController Controller;
    private Vector3 CurrentMoveVelocity;
    private Vector3 MoveDampVelocity;

    private Vector3 CurrentForceVelocity;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        WalkSpeed = 5;
    }

    // Update is called once per frame
    void Update()
    {
        RunSpeed = WalkSpeed * 2;

        Vector3 PlayerInput = new Vector3
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = 0f,
            z = Input.GetAxisRaw("Vertical")
        };

        if (PlayerInput.magnitude > 1f)
            PlayerInput.Normalize();

        bool isMoving = PlayerInput.sqrMagnitude > 0.001f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 MoveVector = transform.TransformDirection(PlayerInput);
        float CurrentSpeed = isRunning ? RunSpeed : WalkSpeed;

        float animSpeed = 0f;
        if (isMoving)
            animSpeed = isRunning ? 1f : 0.5f;

        Debug.Log("Animation Speed" + animSpeed);
        anim.SetFloat("Speed", animSpeed);

        CurrentMoveVelocity = Vector3.SmoothDamp(
            CurrentMoveVelocity,
            MoveVector * CurrentSpeed,
            ref MoveDampVelocity,
            MoveSmoothTime
        );

        Controller.Move(CurrentMoveVelocity * Time.deltaTime);

        Ray groundCheckray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundCheckray, 1.25f))
            CurrentForceVelocity.y = -2f;
        else
            CurrentForceVelocity.y -= GravityStrength * Time.deltaTime;

        Controller.Move(CurrentForceVelocity * Time.deltaTime);
    }

}
