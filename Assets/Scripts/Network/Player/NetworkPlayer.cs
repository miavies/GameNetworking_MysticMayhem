using System;
using Fusion;
using Network;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkPlayer : NetworkBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Jump = Animator.StringToHash("Jump");
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    
    [SerializeField] private Animator _animator;

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }

    [Networked] public NetworkPlayerAnimatorData AnimatorData { get; set; }

    #region Interpolation Variables
    private Vector3 _lastKnownPosition;
    [SerializeField]private float _lerpSpeed = 3f;

    private bool _isCrouching = false;
    #endregion

    #region Fusion Callbacks
    public override void Spawned()
    {
        if (HasInputAuthority) // client
        {
            
        }

        if (HasStateAuthority) // server
        {
            PlayerColor = Random.ColorHSV();

            AnimatorData = new NetworkPlayerAnimatorData()
            {
                Speed = 0,
                Jump = false,
                Forward = false,
                Back = false,
                Left = false,
                Right = false,
                Crouch = false
            };
        }
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (!GetInput(out NetworkInputData input)) return;

        //Movement
        Vector3 move = new Vector3(input.InputVector.x, 0f, input.InputVector.z); 
        transform.position += move * Runner.DeltaTime;

        // Movement direction
        bool movingForward = input.InputVector.z > 0.1f;
        bool movingBack = input.InputVector.z < -0.1f;
        bool movingRight = input.InputVector.x > 0.1f;
        bool movingLeft = input.InputVector.x < -0.1f;

        //Run
        _animator.SetFloat(Speed, input.SprintInput ? 1f : 0f);

        NetworkedPosition = transform.position;
        AnimatorData = new NetworkPlayerAnimatorData()
        {
            Speed = input.SprintInput ? 1f : 0f,
            //Jump = input.JumpInput,
            Forward = movingForward,
            Back = movingBack,
            Left = movingLeft,
            Right = movingRight,
            //Crouch = input.CrouchInput
        };
    }


    public override void Render()
    {
        if (_meshRenderer != null && _meshRenderer.material.color != PlayerColor)
        {
            _meshRenderer.material.color = PlayerColor;
        }

        //Crouching
        _animator.SetBool("Crouching", AnimatorData.Crouch);
        //if (_isCrouching) return;

        //Jump
        if (AnimatorData.Jump)
            _animator.SetTrigger("Jump");

        //Movement
        if (AnimatorData.Forward)
        {
            _animator.SetBool("Forward", true);
        }
        else if (AnimatorData.Back)
        {
            _animator.SetBool("Back", true);
        }
        else if (AnimatorData.Left)
        {
            _animator.SetBool("Left", true);
        }
        else if (AnimatorData.Right)
        {
            _animator.SetBool("Right", true);
        }
        else
        {
            _animator.SetBool("Forward", false);
            _animator.SetBool("Back", false);
            _animator.SetBool("Left", false);
            _animator.SetBool("Right", false);
        }

      

        //Run
        _animator.SetFloat(Speed, AnimatorData.Speed);

        
    }

    public void LateUpdate()
    {
        this.transform.position = Vector3.Lerp(_lastKnownPosition, NetworkedPosition, Runner.DeltaTime * _lerpSpeed);
        _lastKnownPosition = NetworkedPosition;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(Color color)
    {
        if (HasStateAuthority)
        {
            this.PlayerColor = color;
        }
    }
    
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string color)
    {
        if (HasStateAuthority)
        {
            this.PlayerName = color;
        }
        //example of how to use string
        //this.PlayerName.ToString();
    }

    #endregion
    
    #region Unity Callbacks

    private void Update()
    {
        if(!HasInputAuthority) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var randColor = Random.ColorHSV();
            RPC_SetPlayerColor(randColor);
        }
    }
    
    #endregion
    
}
