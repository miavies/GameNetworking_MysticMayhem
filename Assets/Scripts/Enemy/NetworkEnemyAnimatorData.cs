using Fusion;
using UnityEngine;

namespace Network
{
    public struct NetworkEnemyAnimatorData : INetworkStruct
    {
        public NetworkBool Death;
        public NetworkBool Attack;
        public NetworkBool Hit;
        public NetworkBool Walking;
        public NetworkBool Running;
    }
}
