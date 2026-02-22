using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        #region Public Variables

        public static NetworkSessionManager Instance
        {
            get; private set;
        }
        #endregion

        #region Private Variables
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
        private NetworkRunner _networkRunner;
        private List<PlayerRef> _joinedPlayers = new();
        public IReadOnlyList<PlayerRef> JoinedPlayers => _joinedPlayers;

        public event Action<PlayerRef> OnPlayerJoinedEvent;
        public event Action<PlayerRef> OnPlayerLeftEvent;

        private bool aiming;
        private bool aimToggle;
        #endregion

        public async void StartGame(GameMode game)
        {
            _networkRunner = this.gameObject.AddComponent<NetworkRunner>();
            _networkRunner.ProvideInput = true;

            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

            await _networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = game,
                SessionName = "TestSession",
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }

        #region Unity Callbacks

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            #if SERVER
            StartGame(GameMode.Server);
            #elif CLIENT
            StartGame(GameMode.Client);
            #endif
        }
        #endregion

        #region Used Fusion Callbacks
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new NetworkInputData();

            Transform cam = Camera.main.transform;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            data.InputVector = forward * v + right * h;
            data.SprintInput = Input.GetKey(KeyCode.LeftShift);

            data.AimPressed = Input.GetMouseButtonDown(1); 
            data.AimReleased = Input.GetMouseButtonUp(1);   

            data.FirePressed = Input.GetMouseButtonDown(0);

            data.MouseDeltaX = Input.GetAxis("Mouse X");

            data.CameraForward = cam.forward;
            data.CameraPosition = cam.position;

            input.Set(data);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer) return;
            _joinedPlayers.Add(player);
            OnPlayerJoinedEvent?.Invoke(player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _joinedPlayers.Remove(player);
            OnPlayerLeftEvent?.Invoke(player);
        }

        #endregion

        #region Unused Fusion Callbacks
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {

        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {

        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {

        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {

        }


        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnConnectedToServer(NetworkRunner runner)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        #endregion
    }
}
