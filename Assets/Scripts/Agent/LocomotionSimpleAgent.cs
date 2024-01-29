using UnityEngine;
using Pathfinding;
using Pathfinding.Util;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

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
    Seeker seeker;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        agent = GetComponent<RichAI>();
        agent.canMove = false;

        lookAt = GetComponent<LookAt>();

        agentBehavior = GetComponent<AgentBehavior>();
        seeker = GetComponent<Seeker>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (Time.deltaTime <= 0) return;

        agent.MovementUpdate(Time.deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

        Vector3 worldDeltaPosition = nextPosition - transform.position;
        worldDeltaPosition.y = 0;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1f, Time.deltaTime / .15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        Vector2 velocity = smoothDeltaPosition / Time.deltaTime;

        bool isRunning = agentBehavior.CurrentAgentState == AgentBehavior.AgentState.CHASING_PREY || agentBehavior.CurrentAgentState == AgentBehavior.AgentState.RUNNING_FROM_PREDATOR;
        int runSpeedMultiplier = isRunning && !agentBehavior.IsRecovering ? 2 : 1;
        
        animator.SetFloat("speed", agentBehavior.stats.speed * (isRunning ? agentBehavior.stats.speedMultiplyFactor : 1));
        animator.SetBool("isWalking", velocity.magnitude > 0.5f);
        animator.SetFloat("velocityX", velocity.normalized.x * runSpeedMultiplier);
        animator.SetFloat("velocityZ", velocity.normalized.y * runSpeedMultiplier);

        if (lookAt)
            lookAt.lookAtTargetPosition = agent.steeringTarget + transform.forward;

        transform.rotation = nextRotation;
    }

    void OnAnimatorMove()
    {
        if (agent == null) return;

        agent.MovementUpdate(Time.deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);
        // Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, nextRotation, 0.9f);
        agent.FinalizeMovement(new Vector3(animator.rootPosition.x, nextPosition.y, animator.rootPosition.z), nextRotation);
    }

    public void Seek(Vector3 location)
    {
        agent.destination = location;
    }

    private int counter = 0;
    private int recalculatePathInterval = 10;
    public void Flee(Vector3 location)
    {
        counter += 1;
        if (counter % recalculatePathInterval != 0) {
            return;
        }

        FleePath path = FleePath.Construct(transform.position, location, (int)(agentBehavior.stats.fovRange * 1000));
        path.aimStrength = 0.5f;
        path.spread = 4000;
        seeker.StartPath(path);
    }

}
