using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] private float speed = 2;
    [SerializeField] private LayerMask layerMask = -1;
    [SerializeField] private string foodTag = "";
    [SerializeField] private float fovRange = 10f;
    [SerializeField] private float interactRadius = 0.5f;

    [Header("Wander Parameters")]
    [SerializeField] private float wanderRadius = 10;
    [SerializeField] private float wanderDistance = 20;
    [SerializeField] private float wanderJitter = 0.2f;
    [SerializeField] private float wanderTimer = 5.0f;

    public enum Behavior { Pursue, Evade, Wander };
    public Behavior behavior;

    private NavMeshAgent agent;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        wanderCycleTimer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    public void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        Seek(transform.position - fleeVector);
    }

    public void Pursue(Transform target)
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

    public void Evade(Transform target)
    {
        Vector3 targetDirection = target.transform.position - transform.position;

        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
        float targetSpeed = targetAgent == null ? 0 : targetAgent.speed;

        float lookAhead = targetDirection.magnitude / (agent.speed + targetSpeed);

        Flee(target.transform.position + target.transform.forward * lookAhead);
    }


    private float wanderCycleTimer;
    public void Wander()
    {
        wanderCycleTimer += Time.deltaTime;

        if (wanderCycleTimer >= wanderTimer || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            wanderCycleTimer = 0;
        }
    }

    public List<GameObject> FoodInFOVRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRange);
        List<GameObject> foods = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag(foodTag))
            {
                foods.Add(collider.gameObject);
            }
        }

        return foods;
    }

    public bool IsTargetInteractable(GameObject target) {
        return Vector3.Distance(transform.position, target.transform.position) <= interactRadius;
    }

    public void Consume(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        StartCoroutine(HandleEatingAnimations(target));
    }

    IEnumerator HandleEatingAnimations(GameObject target)
    {
        animator.SetBool("isEating", true);
        yield return new WaitForSeconds(1);
        
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(target);
        animator.SetBool("isEating", false);
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);
        return navHit.position;
    }
}
