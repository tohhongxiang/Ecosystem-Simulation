using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] public string foodTag = "";
    [SerializeField] private float fovRange = 10f;
    private float interactRadius;

    [Header("Wander Parameters")]
    [SerializeField] private float wanderRadius = 10;
    [SerializeField] private float wanderTimer = 5.0f;
    private float wanderCycleTimer;

    [Header("Stats")]
    [SerializeField] private float health = 100;
    [SerializeField] private int healthDecayRate = 1;
    [SerializeField] private float stamina = 100;
    [SerializeField] private float staminaDecayRate = 1;

    private NavMeshAgent agent;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        wanderCycleTimer = wanderTimer;
        interactRadius = agent.stoppingDistance + 0.1f;
    }

    void Update() {
        health -= Time.deltaTime * healthDecayRate;
        if (health <= 0) {
            Die();
        }
    }

    public void Die() {
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath() {
        animator.SetBool("isDead", true);

        yield return new WaitForSeconds(1);
        
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(gameObject);
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

    public List<GameObject> GetFoodInFOVRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRange);
        List<GameObject> foods = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag(foodTag) && NavMesh.SamplePosition(collider.transform.position, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
            {
                foods.Add(collider.gameObject);
            }
        }

        foods = foods.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToList();
        return foods;
    }

    public bool IsTargetInteractable(GameObject target) {
        return Vector3.Distance(transform.position, target.transform.position) <= interactRadius;
    }

    public bool IsAtDestination() {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public void Consume(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        StartCoroutine(HandleConsume(target));
    }

    IEnumerator HandleConsume(GameObject target)
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
