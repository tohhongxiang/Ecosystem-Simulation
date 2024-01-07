using UnityEngine;
using Pathfinding;

// coordinates Animator and IAstarAI

[RequireComponent(typeof(LookAt), typeof(Animator), typeof(IAstarAI))]
public class LocomotionSimpleAgent : MonoBehaviour
{
    Animator animator;
    RichAI agent;
    Vector2 smoothDeltaPosition = Vector2.zero;
    LookAt lookAt;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        agent = GetComponent<RichAI>();
        agent.canMove = false;

        lookAt = GetComponent<LookAt>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        agent.canMove = false;

        Vector3 nextPosition;
        Quaternion nextRotation;
        agent.MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);

        Vector3 worldDeltaPosition = nextPosition - transform.position;
        worldDeltaPosition.y = 0;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1.0f, Time.deltaTime/.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        Vector2 velocity = smoothDeltaPosition / Time.deltaTime;

        animator.SetBool("isWalking", velocity.magnitude > 0.5f);
        animator.SetFloat("velocityX", velocity.normalized.x);
        animator.SetFloat("velocityZ", velocity.normalized.y);
        
        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;
        
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, nextRotation, smooth);
        transform.rotation = smoothedRotation;
    }

    void OnAnimatorMove() {
        Vector3 nextPosition;
        Quaternion nextRotation;
        agent.MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);

        agent.FinalizeMovement(new Vector3(animator.rootPosition.x, nextPosition.y, animator.rootPosition.z), animator.rootRotation);
    }

    public void Seek(Vector3 location)
    {
        agent.destination = location;
    }

    public void SeekRun(Vector3 location)
    {
        agent.destination = location;
    }

    public void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        SeekRun(transform.position - fleeVector);
    }

}
