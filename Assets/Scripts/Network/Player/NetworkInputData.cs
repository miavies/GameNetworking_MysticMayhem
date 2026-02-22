using UnityEngine;
using Fusion;

[System.Serializable]
public struct NetworkInputData : INetworkInput
{
    public Vector3 InputVector;
    public bool SprintInput;

    // Mouse & aiming
    public float MouseDeltaX;
    public bool FirePressed;
    public bool AimPressed;
    public bool AimReleased;
    public Vector3 CameraForward;
    public Vector3 CameraPosition;
}