using UnityEngine;
using UnityEngine.AI;

public class EnemyAIMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    [SerializeField] private Transform currentDestination;
    [SerializeField] private string state;

    [Header("Chasing")]
    [SerializeField] private GameObject target;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float fieldOfView = 45f;
    [SerializeField] private float rangeOfView = 8f;
    [SerializeField] private float attackRange = 3f;

    [Header("Attack")]
    [SerializeField] private bool isAttacking = false;
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
        attackTimer -= Time.deltaTime;

        state = CheckState();

        switch (state) 
        {
            case "Patrol":
                Debug.Log("Patrolling");
                agent.speed = 2;
                agent.isStopped = false;
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

        if (isAttacking)
        {
            return "Attack";
        }

        if (IsTargetVisible())
        {
            LookAtTarget();
            return "Chase";
        }

        return "Patrol";
    }

    private void Chase()
    {
        if (target == null) return;

        agent.speed = 6;

        if (Vector3.Distance(transform.position, target.transform.position) <= attackRange && attackTimer <= 0f)
        {
            agent.isStopped = true;
            isAttacking = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
    }

    private void Attack()
    {
        anim.SetTrigger("Attack");

        attackTimer = attackCooldown;
        isAttacking = false;
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

    private void Patrol()
    {
        if (agent.pathPending || !agent.isOnNavMesh)
            return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            var x = Random.Range(-rangeOfView, rangeOfView); 
            var z = Random.Range(-rangeOfView, rangeOfView);

            Vector3 randomPoint = transform.position + new Vector3(x, 0f, z);

            agent.SetDestination(randomPoint);
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
