using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentBehavior : MonoBehaviour
{
    public float speed = 2;
    public LayerMask layerMask = -1;

    [Header("Wander Parameters")]
    public float wanderRadius = 10;
    public float wanderDistance = 20;
    public float wanderJitter = 0.2f;

    public enum Behavior { Pursue, Evade, Wander };
    public Behavior behavior;
    public Transform target;

    private NavMeshAgent agent;

    public float wanderTimer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderCycleTimer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (behavior == Behavior.Pursue)
        {
            Pursue();
        }
        else if (behavior == Behavior.Evade)
        {
            Evade();
        }
        else
        {
            Wander();
        }
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        Seek(transform.position - fleeVector);
    }

    void Pursue()
    {
        Vector3 targetDirection = target.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDirection));

        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
        float targetSpeed = targetAgent == null ? 0 : targetAgent.speed;

        if ((toTarget > 90 && relativeHeading < 20) || targetSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }

        float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);
        Seek(target.transform.position + target.transform.forward * lookAhead);
    }

    void Evade()
    {
        Vector3 targetDirection = target.transform.position - transform.position;

        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
        float targetSpeed = targetAgent == null ? 0 : targetAgent.speed;

        float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);

        Flee(target.transform.position + target.transform.forward * lookAhead);
    }


    private float wanderCycleTimer;
    void Wander()
    {
        wanderCycleTimer += Time.deltaTime;

        if (wanderCycleTimer >= wanderTimer || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            wanderCycleTimer = 0;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);
        return navHit.position;
    }
}
