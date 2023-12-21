using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public enum Gender { MALE, FEMALE };

[System.Serializable]
public class AgentStats
{
    // stats that can change between instances
    public float maxHealth = 100;
    public float maxHunger = 100;
    public float maxThirst = 100;
    public float matingCooldownSeconds = 30;
    public float reproductionTimeSeconds = 5;
    public float growIntoAdultDurationSeconds = 30;
    public Gender gender = Gender.MALE;

    private readonly float maxMaxHealth = 200;
    private readonly float minMaxHealth = 50;
    private readonly float maxMaxHunger = 50;
    private readonly float minMaxHunger = 10;
    private readonly float maxMaxThirst = 50;
    private readonly float minMaxThirst = 10;
    private readonly float maxMatingCooldownSeconds = 100;
    private readonly float minMatingCooldownSeconds = 5;
    private readonly float minReproductionTimeSeconds = 3;
    private readonly float maxReproductionTimeSeconds = 10;
    private readonly float maxGrowIntoAdultDurationSeconds = 100;
    private readonly float minGrowIntoAdultDurationSeconds = 5;

    public AgentStats(float maxHealth, float maxHunger, float maxThirst, float matingCooldownSeconds, float reproductionTimeSeconds, float growIntoAdultDurationSeconds, Gender gender)
    {
        this.maxHealth = Mathf.Clamp(maxHealth, minMaxHealth, maxMaxHealth);
        this.maxHunger = Mathf.Clamp(maxHunger, minMaxHunger, maxMaxHunger);
        this.maxThirst = Mathf.Clamp(maxThirst, minMaxThirst, maxMaxThirst);
        this.matingCooldownSeconds = Mathf.Clamp(matingCooldownSeconds, minMatingCooldownSeconds, maxMatingCooldownSeconds);
        this.reproductionTimeSeconds = Mathf.Clamp(reproductionTimeSeconds, minReproductionTimeSeconds, maxReproductionTimeSeconds);
        this.growIntoAdultDurationSeconds = Mathf.Clamp(growIntoAdultDurationSeconds, minGrowIntoAdultDurationSeconds, maxGrowIntoAdultDurationSeconds);
        this.gender = gender;
    }

    public AgentStats(AgentStats parent1, AgentStats parent2)
    {
        AgentStats[] parents = { parent1, parent2 };

        // choose from one of the parents
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length); // random gender

        maxHealth = parents[Random.Range(0, parents.Length)].maxHealth;
        maxHunger = parents[Random.Range(0, parents.Length)].maxHunger;
        maxThirst = parents[Random.Range(0, parents.Length)].maxThirst;
        matingCooldownSeconds = parents[Random.Range(0, parents.Length)].matingCooldownSeconds;
        reproductionTimeSeconds = parents[Random.Range(0, parents.Length)].reproductionTimeSeconds;
        growIntoAdultDurationSeconds = parents[Random.Range(0, parents.Length)].growIntoAdultDurationSeconds;

        // random perturbations
        maxHealth *= Random.Range(0.95f, 1.05f);
        maxHunger *= Random.Range(0.95f, 1.05f);
        maxThirst *= Random.Range(0.95f, 1.05f);
        matingCooldownSeconds *= Random.Range(0.95f, 1.05f);
        reproductionTimeSeconds *= Random.Range(0.95f, 1.05f);
        growIntoAdultDurationSeconds *= Random.Range(0.95f, 1.05f);

        // clamp
        maxHealth = Mathf.Clamp(maxHealth, minMaxHealth, maxMaxHealth);
        maxHunger = Mathf.Clamp(maxHunger, minMaxHunger, maxMaxHunger);
        maxThirst = Mathf.Clamp(maxThirst, minMaxThirst, maxMaxThirst);
        matingCooldownSeconds = Mathf.Clamp(matingCooldownSeconds, minMatingCooldownSeconds, maxMatingCooldownSeconds);
        reproductionTimeSeconds = Mathf.Clamp(reproductionTimeSeconds, minReproductionTimeSeconds, maxReproductionTimeSeconds);
        growIntoAdultDurationSeconds = Mathf.Clamp(growIntoAdultDurationSeconds, minGrowIntoAdultDurationSeconds, maxGrowIntoAdultDurationSeconds);
    }
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] public string foodTag = "";
    [SerializeField] public string waterTag = "Water";
    [SerializeField] private float fovRange = 10f;
    private float interactRadius;
    private float reproduceRadius;

    [Header("Wander Parameters")]
    [SerializeField] private float wanderRadius = 10;
    [SerializeField] private float wanderTimer = 5.0f;
    private float wanderCycleTimer;

    [Header("Stats")]
    public AgentStats stats;
    private float health;
    public float GetHealth()
    {
        return health;
    }

    private float hunger;
    private readonly float hungerThresholdPercentage = 0.5f;
    public float GetHunger()
    {
        return hunger;
    }
    public bool IsHungry() {
        return hunger <= hungerThresholdPercentage * stats.maxHunger;
    }

    private float thirst;
    private readonly float thirstThresholdPercentage = 0.5f;
    public float GetThirst()
    {
        return thirst;
    }
    public bool IsThirsty() {
        return thirst <= thirstThresholdPercentage * stats.maxThirst;
    }

    private readonly float foodHealthReplenish = 80;
    private readonly float waterHealthReplenish = 80;

    private bool isChild = false;
    private float childCounter = 0;
    private const float childScale = 0.5f;

    private NavMeshAgent agent;
    private Animator animator;

    public enum AgentState { EATING, DONE_EATING, DRINKING, DONE_DRINKING, MATING, DONE_MATING, WANDERING, DEAD };
    private AgentState agentState = AgentState.WANDERING;

    public AgentState GetAgentState()
    {
        return agentState;
    }

    private readonly HashSet<GameObject> blacklistedTargets = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        wanderCycleTimer = wanderTimer;
        interactRadius = agent.stoppingDistance + agent.radius + 0.1f;
        reproduceRadius = agent.stoppingDistance + 2 * agent.radius;

        health = stats.maxHealth;
        hunger = stats.maxHunger;
        thirst = stats.maxThirst;
    }

    void Update()
    {
        HandleHealthUpdate();
        HandleGrowIntoAdultUpdate();
    }

    private void HandleHealthUpdate()
    {
        // decrease hunger and thirst all the time, stopping at 0
        hunger = Mathf.Max(hunger - Time.deltaTime, 0);
        thirst = Mathf.Max(thirst - Time.deltaTime, 0);

        if (hunger <= 0)
        {
            health -= Time.deltaTime;
        }

        if (thirst <= 0)
        {
            health -= Time.deltaTime;
        }

        if (!IsHungry() && !IsThirsty()) {
            health += Time.deltaTime;
            health = Mathf.Min(health, stats.maxHealth);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void HandleGrowIntoAdultUpdate()
    {
        if (isChild)
        {
            childCounter += Time.deltaTime;
            float progressToAdult = childCounter / stats.growIntoAdultDurationSeconds;

            float size = Mathf.Lerp(childScale, 1, progressToAdult);
            transform.localScale = new Vector3(size, size, size);
        }

        if (childCounter >= stats.growIntoAdultDurationSeconds)
        {
            isChild = false;
            transform.localScale = new Vector3(1, 1, 1);
            childCounter = 0;
        }
    }

    public void Die()
    {
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        agentState = AgentState.DEAD;
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
        agentState = AgentState.WANDERING;
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRange, LayerMask.GetMask("Food"));
        List<GameObject> foods = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (
                collider.gameObject.CompareTag(foodTag) &&
                NavMesh.SamplePosition(collider.transform.position, out NavMeshHit hit, agent.stoppingDistance + 0.1f, NavMesh.AllAreas) &&
                !blacklistedTargets.Contains(collider.gameObject)
            )
            {
                foods.Add(collider.gameObject);
            }
        }

        foods = foods.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToList();
        return foods;
    }

    public List<Vector3> GetWaterInFOVRange() // since water is just a single entity, we want to get positions of where we can reach
    {
        List<Vector3> waters = WaterGenerator.Instance.GetAccessibleWaterPoints();
        waters = waters
            .Where((d) => (d - transform.position).sqrMagnitude <= fovRange * fovRange)
            .OrderBy((d) => (d - transform.position).sqrMagnitude).ToList();
        return waters;
    }

    public bool IsTargetInteractable(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= interactRadius;
    }

    public bool IsCoordinateInteractable(Vector3 coordinate)
    {
        return Vector3.Distance(transform.position, coordinate) <= interactRadius * 3;
    }

    public bool IsTargetInReproduceRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= reproduceRadius * 2f;
    }

    public bool IsAtDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public void Eat(GameObject target)
    {
        if (target == null || agentState == AgentState.EATING) // make sure that the current creature is not already consuming
        {
            return;
        }

        StartCoroutine(HandleEat(target));
    }

    IEnumerator HandleEat(GameObject target)
    {
        agentState = AgentState.EATING;
        animator.SetBool("isEating", true);
        target.tag = "Untagged";

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(target);
        animator.SetBool("isEating", false);
        hunger = Mathf.Min(hunger + foodHealthReplenish, stats.maxHunger);
        agentState = AgentState.DONE_EATING;
    }

    public void Drink()
    {
        if (agentState == AgentState.DRINKING)
        {
            return;
        }

        StartCoroutine(HandleDrink());
    }

    IEnumerator HandleDrink()
    {
        agentState = AgentState.DRINKING;
        animator.SetBool("isDrinking", true);

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        animator.SetBool("isDrinking", false);
        thirst = Mathf.Min(thirst + waterHealthReplenish, stats.maxThirst);
        agentState = AgentState.DONE_DRINKING;
    }

    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);
        return navHit.position;
    }

    public List<GameObject> GetMatesInFOVRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRange);
        List<GameObject> mates = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject)
            { // dont get itself
                continue;
            }

            if (collider.gameObject.TryGetComponent<AgentBehavior>(out var partnerAgentBehavior) && partnerAgentBehavior.CanMate() && partnerAgentBehavior.stats.gender != stats.gender)
            {
                mates.Add(collider.gameObject);
            }
        }

        mates = mates.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToList();
        return mates;
    }

    public void Mate(GameObject mate)
    {
        if (mate == null || agentState == AgentState.MATING) // make sure that the current creature is not already consuming
        {
            return;
        }

        StartCoroutine(HandleMatingCooldown(mate));
        StartCoroutine(HandleMating(mate));
    }

    public bool CanMate()
    {
        return !gameObject.CompareTag("Mated") && !isChild;
    }

    IEnumerator HandleMating(GameObject mate)
    {
        if (mate == null)
        {
            yield break;
        }

        agentState = AgentState.MATING;
        animator.SetBool("isMating", true);

        // make both look at each other
        transform.LookAt(mate.transform);
        mate.transform.LookAt(transform);

        // we will choose the maximum of both parents
        // float reproductionTimeSeconds = Mathf.Max(stats.reproductionTimeSeconds, mate.GetComponent<AgentBehavior>().stats.reproductionTimeSeconds);
        yield return new WaitForSeconds(5);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (mate == null)
        {
            yield break;
        }

        animator.SetBool("isMating", false);

        gameObject.tag = "Mated";
        mate.tag = "Mated";
        agentState = AgentState.DONE_MATING;

        if (stats.gender == Gender.FEMALE) // make only the female spawn the child
        {
            GameObject child = Instantiate(gameObject, gameObject.transform.parent);
            child.tag = "Untagged";

            AgentBehavior childAgentBehavior = child.GetComponent<AgentBehavior>();
            childAgentBehavior.isChild = true;
            childAgentBehavior.stats = new AgentStats(mate.GetComponent<AgentBehavior>().stats, stats);
        }
    }

    IEnumerator HandleMatingCooldown(GameObject mate)
    {
        yield return new WaitForSeconds(stats.matingCooldownSeconds);

        if (mate == null)
        { // died
            yield break;
        }

        mate.tag = "Untagged";
        gameObject.tag = "Untagged";
    }

    public void BlacklistTarget(GameObject target)
    {
        blacklistedTargets.Add(target);
    }
}
