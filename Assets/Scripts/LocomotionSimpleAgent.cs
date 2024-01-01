using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LookAt))]
public class LocomotionSimpleAgent : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    LookAt lookAt;


    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = true;

        lookAt = GetComponent<LookAt>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void OnAnimatorMove()
    {
        if (animator == null) { // animator not ready yet
            return;
        }

        // pull animator towards agent (agent controls position, will cause leg sliding)
        // animator.rootPosition = agent.nextPosition;

        // pull agent towards animator (animator controls position)
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

        bool isWalking = velocity.magnitude > 0.5f && agent.remainingDistance > agent.stoppingDistance;
        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("velocityX", velocity.normalized.x);
        animator.SetFloat("velocityZ", velocity.normalized.y);

        float deltaMagnitude = worldDeltaPosition.magnitude;
        if (deltaMagnitude > agent.radius / 2f)
        {
            transform.position = Vector3.Lerp(animator.rootPosition, agent.nextPosition, smooth);
        }

        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

        transform.rotation = agent.transform.rotation;
    }

}
