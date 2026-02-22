using Fusion;
using Fusion.Sockets;
using Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float WalkSpeed = 5f;
    [SerializeField] private float RunSpeed = 10f;
    [SerializeField] private float MoveSmoothTime = 0.1f;
    [SerializeField] private float GravityStrength = 9.81f;

    [Header("Movement Snmoothing")]
    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 velocityDamp;
    private Vector3 gravityVelocity;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    [Networked] public PlayerNetworkAnimatorData AnimatorData { get; set; }


    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        if (HasStateAuthority)
        {
            AnimatorData = new PlayerNetworkAnimatorData()
            {
                Speed = 0f,
                Attack = false,
                Aiming = false,
            };
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            if (!GetInput(out NetworkInputData data))
                return;

            HandleMovement(data, Runner.DeltaTime);
            HandleGravity(Runner.DeltaTime);

            var animData = AnimatorData;
            animData.Speed = data.InputVector.sqrMagnitude > 0.001f ? (data.SprintInput ? 1f : 0.5f) : 0f;
            AnimatorData = animData;
        }

        ApplyAnimatorSpeed();
    }

    private void HandleMovement(NetworkInputData data, float deltaTime)
    {
        Vector3 inputVector = data.InputVector;
        if (inputVector.magnitude > 1f)
            inputVector.Normalize();

        float speed = data.SprintInput ? RunSpeed : WalkSpeed;
        Vector3 move = inputVector;

        currentVelocity = Vector3.SmoothDamp(currentVelocity, move * speed, ref velocityDamp, MoveSmoothTime);
        controller.Move((currentVelocity + gravityVelocity) * deltaTime);

        Vector3 moveFlat = new Vector3(move.x, 0f, move.z);
        if (moveFlat.sqrMagnitude > 0.001f && !AnimatorData.Aiming)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveFlat), 10f * deltaTime);
        }
    }

    private void HandleGravity(float deltaTime)
    {
        gravityVelocity.y = controller.isGrounded ? -2f : gravityVelocity.y - GravityStrength * deltaTime;
    }
    private void ApplyAnimatorSpeed()
    {
        float targetSpeed = AnimatorData.Speed;
        float current = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(current, targetSpeed, 0.05f));
    }

    public override void Render()
    {
       ApplyAnimatorSpeed();
    }
}