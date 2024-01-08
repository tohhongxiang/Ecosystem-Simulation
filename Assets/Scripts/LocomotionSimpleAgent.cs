using UnityEngine;
using Pathfinding;

// coordinates Animator and RichAI

[RequireComponent(typeof(LookAt))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(RichAI))]
[RequireComponent(typeof(AgentBehavior))]
public class LocomotionSimpleAgent : MonoBehaviour
{
    Animator animator;
    RichAI agent;
    Vector2 smoothDeltaPosition = Vector2.zero;
    AgentBehavior agentBehavior;
    LookAt lookAt;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        agent = GetComponent<RichAI>();
        agent.canMove = false;

        lookAt = GetComponent<LookAt>();

        agentBehavior = GetComponent<AgentBehavior>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (Time.deltaTime <= 0) return;

        int timeMultiplier = (int)Mathf.Round(Time.timeScale);
        float timeStep = Time.deltaTime / timeMultiplier;

        for (int i = 0; i < timeStep; i++) {
            agent.MovementUpdate(timeStep, out Vector3 nextPosition, out Quaternion nextRotation);

            Vector3 worldDeltaPosition = nextPosition - transform.position;
            worldDeltaPosition.y = 0;

            float dx = Vector3.Dot(transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            float smooth = Mathf.Min(1f, timeStep / .15f);
            smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            Vector2 velocity = smoothDeltaPosition / timeStep;

            int runSpeedMultiplier = agentBehavior.GetAgentState() == AgentBehavior.AgentState.RUNNING && !agentBehavior.GetIsRecovering() ? 2 : 1;
            animator.SetBool("isWalking", velocity.magnitude > 0.5f);
            animator.SetFloat("velocityX", velocity.normalized.x * runSpeedMultiplier);
            animator.SetFloat("velocityZ", velocity.normalized.y * runSpeedMultiplier);

            if (lookAt)
                lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

            transform.rotation = nextRotation;
        }
    }

    void OnAnimatorMove()
    {
        if (agent == null) return;

        int timeMultiplier = (int)Mathf.Round(Time.timeScale);
        float timeStep = Time.deltaTime / timeMultiplier;

        for (int i = 0; i < timeStep; i++) {
            agent.MovementUpdate(timeStep, out Vector3 nextPosition, out Quaternion nextRotation);
            agent.FinalizeMovement(new Vector3(animator.rootPosition.x, nextPosition.y, animator.rootPosition.z), transform.rotation);
        }
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
