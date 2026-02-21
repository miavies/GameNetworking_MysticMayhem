using Fusion;
using UnityEngine;

namespace Network
{
    public struct NetworkPlayerAnimatorData : INetworkStruct
    {
        public float Speed;
        public NetworkBool Jump;
        public NetworkBool Crouch;
        public NetworkBool Forward;
        public NetworkBool Back;
        public NetworkBool Left;
        public NetworkBool Right;
    }
}