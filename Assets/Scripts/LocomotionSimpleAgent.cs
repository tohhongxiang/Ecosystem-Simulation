using UnityEngine;
using UnityEngine.AI;

public class LocomotionSimpleAgent : MonoBehaviour
{
    public float speed = 2;

    [Header("Wander Parameters")]
    public float wanderRadius = 10;
    public float wanderDistance = 20;
    public float wanderJitter = 0.2f;

    Animator animator;
    NavMeshAgent agent;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    public enum Behavior { Pursue, Evade, Wander };
    public Behavior behavior;
    
    public Transform target;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;
        animator.speed = speed;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        // agent.updateRotation = false;
    }

    void Update()
    {
        if (behavior == Behavior.Pursue)
        {
            Pursue();
        } else if (behavior == Behavior.Evade) {
            Evade();
        } else {
            Wander();
        }

        // Wander();
        UpdateAnimation();
    }

    void OnAnimatorMove()
    {
        Vector3 rootPosition = animator.rootPosition;

        rootPosition.y = agent.nextPosition.y;
        transform.position = rootPosition;
        transform.rotation = animator.rootRotation;
        agent.nextPosition = rootPosition;
    }

    void UpdateAnimation()
    {
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        worldDeltaPosition.y = 0;

        // map worldDeltaPosition to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        velocity = smoothDeltaPosition / Time.deltaTime;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            velocity = Vector2.Lerp(Vector2.zero, velocity, agent.remainingDistance / agent.stoppingDistance);
        }

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.stoppingDistance;
        animator.SetBool("shouldMove", shouldMove);
        animator.SetFloat("velocityX", velocity.normalized.x);
        animator.SetFloat("velocityZ", velocity.normalized.y);

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > agent.radius / 2f)
        {
            transform.position = Vector3.Lerp(animator.rootPosition, agent.nextPosition, smooth);
        }

        LookAt lookAt = GetComponent<LookAt>();
        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

        transform.rotation = agent.transform.rotation;
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


    Vector3 wanderTarget = Vector3.zero;
    void Wander()
    {
        wanderTarget += new Vector3(Random.Range(-1, 1) * wanderJitter, 0, Random.Range(-1, 1) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + Vector3.forward * wanderDistance;
        Vector3 targetWorld = gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }
}
