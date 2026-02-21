using Fusion;
using Network;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIMovement : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    [SerializeField] private string state;
    private float spawnTimer = 3.5f;

    [Header("Patroling")]
    [SerializeField] private float idleTime = 1.5f;
    private float idleTimer = 0f;

    [Header("Chasing")]
    private NetworkObject targetObject;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float fieldOfView = 45f;
    [SerializeField] private float rangeOfView = 8f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float stoppingDistance = 1f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 3f;
    private float attackTimer = 0f;

    [Header("Networked Properties")]
    [Networked] public NetworkEnemyAnimatorData AnimatorData { get; set; }
    [Networked] private int AttackTick { get; set; }
    private int lastAttackTick;


    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (!HasStateAuthority)
            agent.enabled = false;

        if (HasStateAuthority) // server
        {
            AnimatorData = new NetworkEnemyAnimatorData()
            {
                Death = false,
                Attack = false,
                Hit = false,
                Walking = false,
                Running = false
            };
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        RunAI();
    }

    public override void Render()
    {
        anim.SetBool("Running", AnimatorData.Running);
        anim.SetBool("Walking", AnimatorData.Walking);

        if (AttackTick != lastAttackTick)
        {
            anim.SetTrigger("Attack");
            lastAttackTick = AttackTick;
        }
    }

    void RunAI()
    {
        if (spawnTimer > 0f)
        {
            spawnTimer -= Runner.DeltaTime;
            return;
        }

        state = CheckState();

        switch (state)
        {
            case "Patrol":
                Debug.Log("Patrolling");
                Patrol();
                break;

            case "Chase":
                Debug.Log("Chasing");
                Chase();
                break;

            case "Attack":
                Debug.Log("Attacking");
                Attack();
                break;
        }
    }

    private string CheckState()
    {
        CheckIfEnteredRange();

        if (IsTargetVisible() && Vector3.Distance(transform.position, targetObject.transform.position) <= attackRange)
        {
            return "Attack";
        }

        if (IsTargetVisible())
        {
            return "Chase";
        }

        return "Patrol";
    }

    private void CheckIfEnteredRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, rangeOfView, playerLayer);
        float minDist = Mathf.Infinity;
        NetworkObject nearest = null;

        foreach (var hit in hits)
        {
            var netObj = hit.GetComponent<NetworkObject>();
            if (netObj == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = netObj;
            }
        }
        if (nearest != null)
        {
            targetObject = nearest;
        }
    }

    private bool IsTargetVisible()
    {
        if (targetObject == null) return false;

        Vector3 toTarget = targetObject.transform.position - transform.position;
        float distToTarget = toTarget.magnitude;

        //Check if in range
        if (distToTarget > rangeOfView) return false;

        //Check if within FOV
        Vector3 dir = toTarget.normalized;
        float dot = Vector3.Dot(transform.forward, dir);
        float threshold = Mathf.Cos(fieldOfView * Mathf.Deg2Rad);

        if (dot >= threshold) return true;
        else return false;
    }

    void LookAtTarget()
    {
        Vector3 direction = targetObject.transform.position - transform.position;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Chase()
    {
        if (targetObject == null) return;

        LookAtTarget();

        agent.speed = 6;

        agent.isStopped = false;


        float distance = Vector3.Distance(transform.position, targetObject.transform.position);
        float x = transform.position.x;
        float z = transform.position.z;
        float xP = targetObject.transform.position.x;
        float zP = targetObject.transform.position.z;

        float stopX = -(((xP - x) * stoppingDistance) / distance) + xP;
        float stopZ = -(((zP - z) * stoppingDistance) / distance) + zP;

        Vector3 stoppingDestination = new Vector3(stopX, transform.position.y, stopZ);

        AnimatorData = new NetworkEnemyAnimatorData()
        {
            Running = true,
            Walking = false,
            Attack = false
        };

        agent.SetDestination(stoppingDestination);
    }

    private void Attack()
    {
        if (targetObject == null) return;

        LookAtTarget();
        attackTimer -= Runner.DeltaTime;

        AnimatorData = new NetworkEnemyAnimatorData()
        {
            Running = false,
            Walking = false,
            Attack = false
        };

        if (attackTimer <= 0)
        {
            AnimatorData = new NetworkEnemyAnimatorData()
            {
                Running = false,
                Walking = false,
                Attack = true
            };

            AttackTick++;
            attackTimer = attackCooldown;
        }
    }

    public void DamagePlayer()
    {
        if (!HasStateAuthority) return;
        if (targetObject == null) return;

        NetworkHealth playerHealth = targetObject.GetComponent<NetworkHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage((float)attackDamage);
        Debug.Log("Player hit for " + attackDamage + " damage!");
    }

    private void Patrol()
    {
        agent.speed = 2;
        agent.isStopped = false;

        if (agent.pathPending || !agent.isOnNavMesh)
            return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            idleTimer -= Runner.DeltaTime;
            agent.isStopped = true;
            agent.ResetPath();

            AnimatorData = new NetworkEnemyAnimatorData()
            {
                Walking = false,
                Running = false,
                Attack = false
            };

            if (idleTimer <= 0)
            {
                var x = Random.Range(-rangeOfView, rangeOfView);
                var z = Random.Range(-rangeOfView, rangeOfView);

                Vector3 randomPoint = transform.position + new Vector3(x, 0f, z);

                AnimatorData = new NetworkEnemyAnimatorData()
                {
                    Running = false,
                    Walking = true,
                    Attack = false
                };

                agent.SetDestination(randomPoint);
                idleTimer = idleTime;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeOfView);

        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (targetObject == null) return;

        // Line to target
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetObject.transform.position);

        //Forward range
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * rangeOfView);

        /// FOV cone lines
        Gizmos.color = Color.yellow;
        Quaternion leftRot = Quaternion.Euler(0, -fieldOfView, 0);
        Quaternion rightRot = Quaternion.Euler(0, fieldOfView, 0);

        Gizmos.DrawLine(transform.position, transform.position + leftRot * transform.forward * 10);
        Gizmos.DrawLine(transform.position, transform.position + rightRot * transform.forward * 10);
    }


}
