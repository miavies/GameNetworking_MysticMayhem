using Fusion;
using UnityEngine;

namespace Network
{
    public struct PlayerNetworkAnimatorData : INetworkStruct
    {
        public float Speed;
        public NetworkBool Attack;
        public NetworkBool Aiming;
        public NetworkBool Death;
    }
}
