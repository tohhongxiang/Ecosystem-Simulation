using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Pathfinding;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Animator), typeof(LocomotionSimpleAgent), typeof(IAstarAI))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] public string foodTag = "";
    [SerializeField] public string waterTag = "Water";
    [SerializeField] public string predatorTag = "";

    [Header("Stats")]
    public AgentStats stats;

    // internal state
    private float health;
    public float GetHealth()
    {
        return health;
    }

    private float hunger;
    private readonly float hungerThresholdPercentage = 0.8f;
    public float GetHunger()
    {
        return hunger;
    }
    public bool IsHungry()
    {
        return hunger <= hungerThresholdPercentage * stats.maxHunger;
    }

    private float thirst;
    private readonly float thirstThresholdPercentage = 0.8f;
    public float GetThirst()
    {
        return thirst;
    }
    public bool IsThirsty()
    {
        return thirst <= thirstThresholdPercentage * stats.maxThirst;
    }

    private float stamina;
    public float GetStamina()
    {
        return stamina;
    }
    private bool isRecovering = false;
    public bool GetIsRecovering()
    {
        return isRecovering;
    }
    private readonly float staminaRecoveryThreshold = 0.9f;

    private bool justMatedRecently = false;
    public bool IsJustMatedRecently()
    {
        return justMatedRecently;
    }
    public void SetMated(bool mated)
    {
        justMatedRecently = mated;
    }

    public enum AgentState
    {
        GOING_TO_FOOD, EATING, DONE_EATING,
        GOING_TO_WATER, DRINKING, DONE_DRINKING,
        GOING_TO_MATE, MATING, DONE_MATING,
        ATTACKING, DONE_ATTACKING,
        WANDERING, RUNNING, DEAD
    };
    private AgentState agentState = AgentState.WANDERING;
    public AgentState GetAgentState()
    {
        return agentState;
    }

    private bool isChild = false;
    private float childCounter = 0;
    private const float childScale = 0.5f;

    private readonly int maxCandidates = 10;

    private RichAI agent;
    private Animator animator;
    private LocomotionSimpleAgent locomotionSimpleAgent;
    private readonly HashSet<Vector3> blacklistedWaterPoints = new HashSet<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<RichAI>();
        agent.maxSpeed = 1;
        agent.rotationSpeed = stats.speed * agent.rotationSpeed;

        animator = GetComponent<Animator>();
        animator.SetFloat("speed", stats.speed);

        locomotionSimpleAgent = GetComponent<LocomotionSimpleAgent>();

        health = stats.maxHealth;
        hunger = stats.maxHunger;
        thirst = stats.maxThirst;
        stamina = stats.maxStamina;
    }

    void Update()
    {
        UpdateStats();
        HandleGrowIntoAdultUpdate();
    }

    private void UpdateStats()
    {
        // decrease hunger and thirst all the time, stopping at 0
        hunger = Mathf.Max(hunger - Time.deltaTime * stats.speed * stats.speed, 0);
        thirst = Mathf.Max(thirst - Time.deltaTime * stats.speed * stats.speed, 0);

        if (agentState == AgentState.RUNNING && agent.velocity.magnitude > 0.3f)
        {
            stamina = Mathf.Max(stamina - Time.deltaTime, 0);
            if (stamina == 0 && !isRecovering)
            {
                isRecovering = true;
                StartCoroutine(HandleCheckIfRecovered());
            }
        }
        else
        {
            stamina = Mathf.Min(stamina + Time.deltaTime, stats.maxStamina);
        }

        // if (hunger <= 0 || thirst <= 0)
        // {
        //     health -= Time.deltaTime;
        // }

        // if (!IsHungry() && !IsThirsty())
        // {
        //     health += Time.deltaTime;
        //     health = Mathf.Min(health, stats.maxHealth);
        // }

        if (health <= 0 || hunger <= 0 || thirst <= 0)
        {
            Die();
        }
    }

    IEnumerator HandleCheckIfRecovered()
    {
        while (stamina < staminaRecoveryThreshold * stats.maxStamina)
        {
            yield return new WaitForSeconds(0.5f);
        }

        isRecovering = false;
    }

    private void HandleGrowIntoAdultUpdate()
    {
        if (!isChild)
        {
            return;
        }

        childCounter += Time.deltaTime;
        float progressToAdult = childCounter / stats.growIntoAdultDurationSeconds;

        float size = Mathf.Lerp(childScale, 1, progressToAdult);
        transform.localScale = new Vector3(size, size, size);

        if (childCounter >= stats.growIntoAdultDurationSeconds)
        {
            isChild = false;
            transform.localScale = new Vector3(1, 1, 1);
            childCounter = 0;
        }
    }

    private void Die()
    {
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        agentState = AgentState.DEAD;
        agent.canMove = false;
        animator.SetBool("isDead", true);

        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }

    public void GoToFood(GameObject target)
    {
        agentState = AgentState.GOING_TO_FOOD;
        locomotionSimpleAgent.Seek(target.transform.position);
    }

    public void GoToWater(Vector3 waterLocation)
    {
        agentState = AgentState.GOING_TO_WATER;
        locomotionSimpleAgent.Seek(waterLocation);
    }

    public void GoToMate(GameObject mate)
    {
        agentState = AgentState.GOING_TO_MATE;
        locomotionSimpleAgent.Seek(mate.transform.position);
    }

    public void Pursue(GameObject target)
    {
        agentState = AgentState.RUNNING;
        Vector3 targetDirection = target.transform.position - transform.position;
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDirection));

        IAstarAI targetAgent = target.GetComponent<IAstarAI>();
        float targetSpeed = targetAgent == null ? 0 : targetAgent.velocity.magnitude;

        if ((toTarget > 90 && relativeHeading < 20) || targetSpeed < 0.01f)
        {
            locomotionSimpleAgent.SeekRun(target.transform.position);
            return;
        }

        float lookAhead = targetDirection.magnitude / (agent.maxSpeed + targetSpeed);
        locomotionSimpleAgent.SeekRun(target.transform.position + target.transform.forward * lookAhead);
    }

    public void Evade(GameObject target)
    {
        agentState = AgentState.RUNNING;
        Vector3 targetDirection = target.transform.position - transform.position;

        IAstarAI targetAgent = target.GetComponent<IAstarAI>();
        float targetSpeed = targetAgent == null ? 0 : targetAgent.velocity.magnitude;

        float lookAhead = targetDirection.magnitude / (agent.maxSpeed + targetSpeed);
        Vector3 targetToRunTo = target.transform.position + target.transform.forward * lookAhead;

        if (Vector3.Distance(targetToRunTo, transform.position) < 1.0f)
        { // if stuck, choose a random direction to run towards
            targetToRunTo = RandomPoint();
        }

        locomotionSimpleAgent.Flee(targetToRunTo);
    }

    public void Wander()
    {
        agentState = AgentState.WANDERING;
        if (!agent.pathPending && (agent.reachedEndOfPath || !agent.hasPath))
        {
            agent.destination = RandomPoint();
            agent.SearchPath();
        }
    }

    public List<GameObject> GetFoodInFOVRange()
    {
        Collider[] colliders = new Collider[maxCandidates];
        Physics.OverlapSphereNonAlloc(transform.position, stats.fovRange, colliders, LayerMask.GetMask(foodTag));
        List<GameObject> foods = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider != null)
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
            .Where((d) => (d - transform.position).sqrMagnitude <= stats.fovRange * stats.fovRange && !blacklistedWaterPoints.Contains(d))
            .OrderBy((d) => (d - transform.position).sqrMagnitude).ToList();
        return waters;
    }

    public List<GameObject> GetPredatorsInFOVRange()
    {
        Collider[] colliders = new Collider[maxCandidates];
        Physics.OverlapSphereNonAlloc(transform.position, stats.fovRange, colliders, LayerMask.GetMask(predatorTag));
        List<GameObject> predators = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }
            // TODO add if predator is chasing self
            predators.Add(collider.gameObject);
        }

        predators = predators.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToList();
        return predators;
    }

    public List<GameObject> GetMatesInFOVRange()
    {
        Collider[] colliders = new Collider[maxCandidates];
        // get only the same species which will be on the same layer. Bitshift used to convert layer to layermask
        Physics.OverlapSphereNonAlloc(transform.position, stats.fovRange, colliders, 1 << gameObject.layer);
        List<GameObject> mates = new List<GameObject>();

        foreach (Collider collider in colliders)
        {
            if (collider == null || collider.gameObject == gameObject)
            { // dont get itself
                continue;
            }

            AgentBehavior partnerAgentBehavior = collider.gameObject.GetComponent<AgentBehavior>();
            if (!partnerAgentBehavior)
            {
                continue;
            }
            bool sameGender = partnerAgentBehavior.stats.gender == stats.gender;
            bool partnerCanMate = partnerAgentBehavior.CanMate();

            if (partnerCanMate && !sameGender)
            {
                mates.Add(collider.gameObject);
            }
        }

        mates = mates.OrderBy((d) => (d.transform.position - transform.position).sqrMagnitude).ToList();
        return mates;
    }

    public bool IsTargetInteractable(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= 2 * agent.radius;
    }

    public bool IsTargetInAttackRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 2;
    }

    public bool IsCoordinateInteractable(Vector3 coordinate)
    {
        return Vector3.Distance(transform.position, coordinate) <= agent.radius * 3;
    }

    public bool IsTargetInReproduceRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 6;
    }

    public bool IsAtDestination()
    {
        return agent.reachedEndOfPath;
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
        agent.isStopped = true;
        agentState = AgentState.EATING;
        animator.SetBool("isEating", true);
        target.layer = LayerMask.NameToLayer("Default");

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(target);
        animator.SetBool("isEating", false);
        hunger = stats.maxHunger;
        agentState = AgentState.DONE_EATING;

        agent.isStopped = false;
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
        agent.isStopped = true;
        agentState = AgentState.DRINKING;
        animator.SetBool("isDrinking", true);

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        animator.SetBool("isDrinking", false);
        thirst = stats.maxThirst;
        agentState = AgentState.DONE_DRINKING;
        agent.isStopped = false;
    }

    private Vector3 RandomPoint()
    {
        Vector3 point = Random.insideUnitSphere * stats.fovRange;

        point.y = 0;
        point += agent.position;
        return point;
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
        return !justMatedRecently && !isChild && !IsHungry() && !IsThirsty();
    }

    IEnumerator HandleMating(GameObject mate)
    {
        if (mate == null)
        {
            yield break;
        }

        agent.isStopped = true;
        agentState = AgentState.MATING;
        animator.SetBool("isMating", true);

        // make both look at each other
        transform.LookAt(mate.transform);
        mate.transform.LookAt(transform);

        // we will choose the maximum of both parents
        float reproductionTimeSeconds = Mathf.Max(stats.reproductionTimeSeconds, mate.GetComponent<AgentBehavior>().stats.reproductionTimeSeconds);
        yield return new WaitForSeconds(reproductionTimeSeconds);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (mate == null)
        {
            yield break;
        }

        animator.SetBool("isMating", false);

        SetMated(true);

        agentState = AgentState.DONE_MATING;

        if (stats.gender == Gender.FEMALE) // make only the female spawn the child
        {
            GameObject child = Instantiate(gameObject, gameObject.transform.parent);
            child.tag = "Untagged";

            AgentBehavior childAgentBehavior = child.GetComponent<AgentBehavior>();
            childAgentBehavior.isChild = true;
            childAgentBehavior.stats = new AgentStats(mate.GetComponent<AgentBehavior>().stats, stats);
        }

        agent.isStopped = false;
    }

    IEnumerator HandleMatingCooldown(GameObject mate)
    {
        yield return new WaitForSeconds(stats.matingCooldownSeconds);

        if (mate == null)
        { // died
            yield break;
        }

        SetMated(false);
    }

    public void Attack(GameObject target)
    {
        if (agentState == AgentState.ATTACKING)
        {
            return;
        }

        StartCoroutine(HandleAttack(target));
    }

    IEnumerator HandleAttack(GameObject target)
    {
        agent.isStopped = true;
        agentState = AgentState.ATTACKING;

        transform.LookAt(target.transform);

        animator.SetBool("isAttacking", true);

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (target != null)
        {
            target.GetComponent<AgentBehavior>().Kill();
            hunger = stats.maxHunger;
        }

        animator.SetBool("isAttacking", false);
        agentState = AgentState.DONE_ATTACKING;
        agent.isStopped = false;
    }

    public void Kill()
    {
        Die();
    }

    private readonly float forgetTime = 90f;
    public void BlacklistTarget(GameObject target)
    {
        target.layer = LayerMask.NameToLayer("Default");
    }

    public void BlacklistWaterPoint(Vector3 target)
    {
        blacklistedWaterPoints.Add(target);
        StartCoroutine(ForgetBlacklistedWaterPoint(target));
    }

    IEnumerator ForgetBlacklistedWaterPoint(Vector3 target)
    {
        yield return new WaitForSeconds(forgetTime);

        blacklistedWaterPoints.Remove(target);
    }

    void OnDrawGizmosSelected()
    {
        if (agent == null) return;
        // DebugFood();
        // DebugWater();
        // DebugMating();

        if (foodTag == "Deer")
        {
            DebugFood();
        } else {
            DebugPredators();
        }
    }

    void DebugFOVRange()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, stats.fovRange);
    }

    void DebugFood()
    {
        var foods = GetFoodInFOVRange();
        foreach (var food in foods)
        {
            Gizmos.DrawLine(transform.position, food.transform.position);
        }
    }

    void DebugWater()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        var waterPoints = GetWaterInFOVRange();

        if (waterPoints.Count > 0)
        {
            Gizmos.color = new Color(1, 0, 1);
            Gizmos.DrawLine(transform.position, waterPoints[0]);
        }

    }

    void DebugMating()
    {
        if (IsJustMatedRecently())
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
        }

        Gizmos.DrawSphere(transform.position, agent.radius * 6);

        var mates = GetMatesInFOVRange();
        foreach (var mate in mates)
        {
            Gizmos.DrawLine(transform.position, mate.transform.position);
        }
    }

    void DebugPredators()
    {
        var predators = GetPredatorsInFOVRange();
        foreach (var predator in predators)
        {
            Gizmos.DrawLine(transform.position, predator.transform.position);
        }
    }
}
