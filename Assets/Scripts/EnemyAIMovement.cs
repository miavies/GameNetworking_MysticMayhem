using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    [SerializeField] private Transform currentDestination;
    [SerializeField] private string state;

    [Header("Patroling")]
    [SerializeField] private float idleTime = 1.5f;
    private float idleTimer = 0f;

    [Header("Chasing")]
    [SerializeField] private GameObject target;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float fieldOfView = 45f;
    [SerializeField] private float rangeOfView = 8f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float stoppingDistance = 1f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    private float attackTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
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

        if (IsTargetVisible() && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
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
        target = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = hit.gameObject;
            }
        }
    }

    private bool IsTargetVisible()
    {
        if (target == null) return false;

        Vector3 toTarget = target.transform.position - transform.position;
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
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Chase()
    {
        if (target == null) return;

        LookAtTarget();

        agent.speed = 6;

        agent.isStopped = false;


        float distance = Vector3.Distance(transform.position, target.transform.position);
        float x = transform.position.x;
        float z = transform.position.z;
        float xP = target.transform.position.x;
        float zP = target.transform.position.z;

        float stopX = -(((xP-x)*stoppingDistance)/distance) + xP;
        float stopZ = -(((zP - z) * stoppingDistance) / distance) + zP;

        Vector3 stoppingDestination = new Vector3(stopX, transform.position.y, stopZ);
        agent.SetDestination(stoppingDestination);
    }

    private void Attack()
    {
        if (target == null) return;

        LookAtTarget();
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            Debug.Log("Animation: Attack");
            //anim.SetTrigger("Attack");
            attackTimer = attackCooldown;
        }
    }

    private void Patrol()
    {
        agent.speed = 2;
        agent.isStopped = false;

        if (agent.pathPending || !agent.isOnNavMesh)
            return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            idleTimer -= Time.deltaTime;
            agent.isStopped = true;
            agent.ResetPath();
            Debug.Log("Animation: Idle");
            //anim.SetBool("Walking", false);

            if (idleTimer <= 0)
            {
                var x = Random.Range(-rangeOfView, rangeOfView);
                var z = Random.Range(-rangeOfView, rangeOfView);

                Vector3 randomPoint = transform.position + new Vector3(x, 0f, z);
                Debug.Log("Animation: Walking");
                //anim.SetBool("Walking", true);
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

        if (target == null) return;

        // Line to target
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.transform.position);

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
