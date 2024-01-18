using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Pathfinding;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LocomotionSimpleAgent))]
[RequireComponent(typeof(RichAI))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // used to instantiate children
    [SerializeField] private string speciesName;
    public string foodTag = "";
    public string predatorTag = "";

    [Header("Stats")]
    public AgentStats stats;

    // internal state
    public float Health { get; private set; } = 100;
    public float MaxHealth { get; private set; } = 100;

    public float Hunger { get; private set; } = 100;
    public float MaxHunger { get; private set; } = 100;
    private readonly float hungerThresholdPercentage = 0.75f;
    public bool IsHungry()
    {
        return Hunger <= hungerThresholdPercentage * MaxHunger;
    }

    public float Thirst { get; private set; } = 100;
    public float MaxThirst { get; private set; } = 100;
    private readonly float thirstThresholdPercentage = 0.75f;
    public bool IsThirsty()
    {
        return Thirst <= thirstThresholdPercentage * MaxThirst;
    }

    public float Stamina { get; private set; } = 0;
    public float MaxStamina { get; private set; } = 100;
    public bool IsRecovering { get; private set; } = false;
    private readonly float staminaRecoveryThreshold = 0.9f;

    public float Age { get; private set; } = 0;

    public enum AgentState
    {
        GOING_TO_FOOD, EATING, DONE_EATING,
        GOING_TO_WATER, DRINKING, DONE_DRINKING,
        GOING_TO_MATE, MATING, DONE_MATING,
        ATTACKING, DONE_ATTACKING,
        WANDERING, DEAD,
        CHASING_PREY, RUNNING_FROM_PREDATOR
    };
    public AgentState CurrentAgentState { get; private set; } = AgentState.WANDERING;

    private bool isChild = false;
    private float childCounter = 0;
    private float childScale = 0.5f;

    private readonly int maxCandidates = 10;

    private RichAI agent;
    private Animator animator;
    private LocomotionSimpleAgent locomotionSimpleAgent;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("speed", stats.speed);

        agent = GetComponent<RichAI>();
        agent.maxSpeed = 1;

        locomotionSimpleAgent = GetComponent<LocomotionSimpleAgent>();

        MaxHealth *= stats.size;
        MaxThirst *= Mathf.Pow(stats.size, 3);
        MaxHunger *= Mathf.Pow(stats.size, 3);

        Health = MaxHealth;
        Hunger = MaxHunger;
        Thirst = MaxThirst;
        Stamina = MaxStamina;

        childScale = 0.5f * stats.size;
        transform.localScale = new Vector3(stats.size, stats.size, stats.size);
    }

    void Update()
    {
        if (CurrentAgentState == AgentState.DEAD) // stop updating when agent is dead
        {
            return;
        }

        UpdateStats();
        HandleGrowIntoAdultUpdate();
        HandleAge();
    }

    private void UpdateStats()
    {
        float netHungerDecrease = (Mathf.Pow(stats.size, 3) + Mathf.Pow(stats.speed, 2)) * stats.hungerDecreaseFactor;
        Hunger = Mathf.Max(Hunger - Time.deltaTime * netHungerDecrease, 0);

        float netThirstDecrease = (Mathf.Pow(stats.size, 3) + Mathf.Pow(stats.speed, 2)) * stats.thirstDecreaseFactor;
        Thirst = Mathf.Max(Thirst - Time.deltaTime * netThirstDecrease, 0);

        bool isRunning = (CurrentAgentState == AgentState.CHASING_PREY || CurrentAgentState == AgentState.RUNNING_FROM_PREDATOR) && agent.velocity.magnitude > 0.3f;
        if (isRunning)
        {
            Stamina = Mathf.Max(Stamina - Time.deltaTime * stats.staminaDecreaseFactor, 0);
            if (Stamina == 0 && !IsRecovering)
            {
                IsRecovering = true;
                StartCoroutine(HandleCheckIfRecovered());
            }
        }
        else
        {
            Stamina = Mathf.Min(Stamina + Time.deltaTime * stats.staminaIncreaseFactor, MaxStamina);
        }

        if (Hunger <= 0 || Thirst <= 0) // if starving or dehydrated, lose health
        {
            Health = Mathf.Max(0, Health - Time.deltaTime);
        }
        else if (!IsHungry() && !IsThirsty()) // if satisfied, recover health
        {
            Health = Mathf.Min(Health + Time.deltaTime, MaxHealth);
        }

        // since dying is handled with a coroutine, we need to ensure this is only called once
        if (Health <= 0)
        {
            CurrentAgentState = AgentState.DEAD;

            if (Hunger <= 0)
            {
                AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsByHunger", 1);
            }

            if (Thirst <= 0)
            {
                AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsByThirst", 1);
            }

            AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsTotal", 1);
            Die();
        }
    }

    IEnumerator HandleCheckIfRecovered()
    {
        while (Stamina < staminaRecoveryThreshold * MaxStamina)
        {
            yield return new WaitForSeconds(0.5f);
        }

        IsRecovering = false;
    }

    private void HandleGrowIntoAdultUpdate()
    {
        if (!isChild)
        {
            return;
        }

        childCounter += Time.deltaTime;
        float progressToAdult = childCounter / stats.growIntoAdultDurationSeconds;

        float size = Mathf.Lerp(childScale, stats.size, progressToAdult);
        transform.localScale = new Vector3(size, size, size);

        if (childCounter >= stats.growIntoAdultDurationSeconds)
        {
            isChild = false;
            transform.localScale = new Vector3(stats.size, stats.size, stats.size);
            childCounter = 0;
        }
    }

    private float checkDeathCounter = 0;
    private readonly float checkDeathInterval = 30;
    private readonly float probabilityOfDyingWhenBorn = 0.01f;
    private readonly float probabilityOfDyingWhenAtExpectedAge = 0.25f;
    private void HandleAge()
    {
        Age += Time.deltaTime;
        checkDeathCounter += Time.deltaTime;

        if (checkDeathCounter > checkDeathInterval)
        {
            checkDeathCounter = 0;

            // when age = 0, this evaluates to probabilityOfDyingWhenBorn
            // when age = maxAge, this evaluates to 0.5
            float a = -Mathf.Log(probabilityOfDyingWhenBorn);
            float b = stats.expectedAge * Mathf.Log(probabilityOfDyingWhenBorn) / (Mathf.Log(probabilityOfDyingWhenBorn) - Mathf.Log(probabilityOfDyingWhenAtExpectedAge));
            float probabilityOfDying = Mathf.Exp(a * (Age / b - 1));
            if (RollForChance(probabilityOfDying))
            {
                DieByAge();
            }
        }

    }

    #region Handle Dying
    private void DieByAge()
    {
        AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsByAge", 1);
        AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsTotal", 1);

        Die();
    }

    private void Die()
    {
        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        CurrentAgentState = AgentState.DEAD;
        agent.canMove = false;
        animator.SetBool("isDead", true);

        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }

    #endregion

    #region Handle Wandering

    public void Wander()
    {
        CurrentAgentState = AgentState.WANDERING;
        if (agent.pathPending || (!agent.reachedEndOfPath && agent.hasPath))
        {
            return;
        }

        float searchRange = IsHungry() || IsThirsty() ? stats.fovRange * 3 : stats.fovRange;
        agent.destination = RandomPoint(searchRange);
        agent.SearchPath();
    }

    private Vector3 RandomPoint(float range)
    {
        Vector3 point = Random.insideUnitSphere * range;

        point.y = 0;
        point += agent.position;
        return point;
    }

    #endregion

    #region Handle Food

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

    public void GoToFood(GameObject target)
    {
        CurrentAgentState = AgentState.GOING_TO_FOOD;
        locomotionSimpleAgent.Seek(target.transform.position);
    }

    public void Eat(GameObject target)
    {
        if (target == null || CurrentAgentState == AgentState.EATING) // make sure that the current creature is not already consuming
        {
            return;
        }

        StartCoroutine(HandleEat(target));
    }

    IEnumerator HandleEat(GameObject target)
    {
        agent.isStopped = true;
        CurrentAgentState = AgentState.EATING;
        transform.LookAt(target.transform);
        animator.SetBool("isEating", true);
        target.layer = LayerMask.NameToLayer("Default");

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(target);
        animator.SetBool("isEating", false);

        Hunger = MaxHunger;
        CurrentAgentState = AgentState.DONE_EATING;

        agent.isStopped = false;
    }

    public void BlacklistTarget(GameObject target)
    {
        target.layer = LayerMask.NameToLayer("Default");
    }

    #endregion

    #region Handle Drink
    public void GoToWater(Vector3 waterLocation)
    {
        CurrentAgentState = AgentState.GOING_TO_WATER;
        locomotionSimpleAgent.Seek(waterLocation);
    }

    public List<Vector3> GetWaterInFOVRange() // since water is just a single entity, we want to get positions of where we can reach
    {
        return WaterGenerator.Instance.GetAccessibleWaterPoints()
            .Where((d) => (d - transform.position).sqrMagnitude <= stats.fovRange * stats.fovRange)
            .OrderBy((d) => (d - transform.position).sqrMagnitude).ToList();
    }

    public void Drink(Vector3 waterPoint)
    {
        if (CurrentAgentState == AgentState.DRINKING)
        {
            return;
        }

        StartCoroutine(HandleDrink(waterPoint));
    }

    IEnumerator HandleDrink(Vector3 waterPoint)
    {
        agent.isStopped = true;
        CurrentAgentState = AgentState.DRINKING;
        transform.LookAt(waterPoint);
        animator.SetBool("isDrinking", true);

        yield return new WaitForSeconds(1);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
        }

        animator.SetBool("isDrinking", false);
        Thirst = MaxThirst;
        CurrentAgentState = AgentState.DONE_DRINKING;
        agent.isStopped = false;
    }

    public void BlacklistWaterPoint(Vector3 target)
    {
        WaterGenerator.Instance.ClearWaterPoint(target);
    }

    #endregion

    #region Handle Mating
    public bool IsPregnant { get; private set; } = false;
    public bool IsRecoveringFromBirth { get; private set; } = false;
    public bool CanMate()
    {
        return !isChild && !IsPregnant && !IsRecoveringFromBirth && !IsHungry() && !IsThirsty();
    }
    public bool IsTargetInReproduceRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 6;
    }

    public void GoToMate(GameObject mate)
    {
        CurrentAgentState = AgentState.GOING_TO_MATE;
        locomotionSimpleAgent.Seek(mate.transform.position);
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

    public void Mate(GameObject mate)
    {
        if (mate == null || CurrentAgentState == AgentState.MATING) // make sure that the current creature is not already consuming
        {
            return;
        }

        CurrentAgentState = AgentState.MATING;
        StartCoroutine(HandleMating(mate));
    }

    private bool RollForChance(float chance)
    {
        return chance > Random.Range(0f, 1f);
    }

    private float reproductionTimeSeconds = 5;
    IEnumerator HandleMating(GameObject mate)
    {
        if (mate == null)
        {
            yield break;
        }

        agent.isStopped = true;
        animator.SetBool("isMating", true);

        // make both look at each other
        transform.LookAt(mate.transform);

        yield return new WaitForSeconds(reproductionTimeSeconds);

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || animator.IsInTransition(0)) // while animation is not finished
        {
            yield return new WaitForSeconds(0.1f);
            if (animator == null)
            {
                yield break; // the agent died
            }
        }

        CurrentAgentState = AgentState.DONE_MATING;
        animator.SetBool("isMating", false);
        agent.isStopped = false;

        if (mate == null)
        {
            yield break;
        }

        if (stats.gender == Gender.FEMALE) // make only the female spawn the child
        {
            StartCoroutine(HandlePregnancy(mate));
        }
    }

    float probabilityOfSpawningExtraChildren = 0.2f;
    IEnumerator HandlePregnancy(GameObject mate)
    {
        AgentStats mateStats = mate.GetComponent<AgentBehavior>().stats;

        IsPregnant = true;
        yield return new WaitForSeconds(stats.gestationDuration);

        int numberOfChildren = 1;
        while (RollForChance(probabilityOfSpawningExtraChildren))
        {
            numberOfChildren += 1;
        }

        for (int i = 0; i < numberOfChildren; i++)
        {
            GameObject child = Instantiate(prefab, gameObject.transform.parent);
            child.tag = "Untagged";

            AgentBehavior childAgentBehavior = child.GetComponent<AgentBehavior>();
            childAgentBehavior.isChild = true;
            childAgentBehavior.stats = new AgentStats(mateStats, stats);

            // disable showing stats
            child.GetComponent<HandleDisplayStats>().enabled = false;
            child.transform.GetChild(1).gameObject.SetActive(false);
        }

        AgentStatsLogger.Instance.AddCountToEvent(speciesName, "births", numberOfChildren);

        IsPregnant = false;

        StartCoroutine(HandleRecoverFromPregnancy());
    }

    IEnumerator HandleRecoverFromPregnancy()
    {
        IsRecoveringFromBirth = true;
        yield return new WaitForSeconds(stats.durationBetweenPregnancies);

        IsRecoveringFromBirth = false;
    }

    #endregion

    #region Handle Predators

    public bool IsTargetInAttackRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 4;
    }

    public void Pursue(GameObject target)
    {
        CurrentAgentState = AgentState.CHASING_PREY;
        locomotionSimpleAgent.Seek(target.transform.position);
    }

    public void Evade(GameObject target)
    {
        CurrentAgentState = AgentState.RUNNING_FROM_PREDATOR;
        Vector3 directionToRunTowards = (transform.position - target.transform.position + transform.position).normalized;
        directionToRunTowards *= stats.fovRange;

        locomotionSimpleAgent.Seek(AstarPath.active.GetNearest(directionToRunTowards).position);
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

    public void Attack(GameObject target)
    {
        if (CurrentAgentState == AgentState.ATTACKING)
        {
            return;
        }

        StartCoroutine(HandleAttack(target));
    }

    IEnumerator HandleAttack(GameObject target)
    {
        agent.isStopped = true;
        CurrentAgentState = AgentState.ATTACKING;

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
            Hunger = MaxHunger;
        }

        animator.SetBool("isAttacking", false);
        CurrentAgentState = AgentState.DONE_ATTACKING;
        agent.isStopped = false;
    }

    public void Kill()
    {
        AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsByHunt", 1);
        AgentStatsLogger.Instance.AddCountToEvent(speciesName, "deathsTotal", 1);
        Die();
    }

    #endregion

    public bool IsCoordinateInteractable(Vector3 coordinate)
    {
        return Vector3.Distance(transform.position, coordinate) <= agent.radius * 4;
    }

    public bool IsAtDestination()
    {
        return !agent.pathPending && agent.reachedEndOfPath;
    }

    public bool IsPathPossible(Vector3 position)
    {
        GraphNode currentPositionNode = AstarPath.active.GetNearest(transform.position).node;
        GraphNode targetPositionNode = AstarPath.active.GetNearest(position).node;

        return PathUtilities.IsPathPossible(currentPositionNode, targetPositionNode);
    }

    #region Debugging
    void OnDrawGizmosSelected()
    {
        if (agent == null) return;

        DebugFood();
        DebugWater();
        DebugMating();
        DebugPredators();
    }

    void DebugFOVRange()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, stats.fovRange);
    }

    void DebugFood()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);

        var foods = GetFoodInFOVRange();
        foreach (var food in foods)
        {
            Gizmos.DrawLine(transform.position, food.transform.position);
        }

        GameObject m = (GameObject)GetComponent<BehaviorTree.Tree>().Root().GetData("target");
        if (m != null)
        {
            Gizmos.DrawSphere(m.transform.position, 2);
        }
    }

    void DebugWater()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);

        var waterPoints = GetWaterInFOVRange();
        foreach (var waterPoint in waterPoints)
        {
            Gizmos.DrawLine(transform.position, waterPoint);
        }

        object m = GetComponent<BehaviorTree.Tree>().Root().GetData("water");
        if (m != null)
        {
            Gizmos.DrawSphere((Vector3)m, 2);
        }
    }

    void DebugMating()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        var mates = GetMatesInFOVRange();
        foreach (var mate in mates)
        {
            Gizmos.DrawLine(transform.position, mate.transform.position);
        }

        GameObject m = (GameObject)GetComponent<BehaviorTree.Tree>().Root().GetData("mate");
        if (m != null)
        {
            Gizmos.DrawSphere(m.transform.position, 2);
        }
    }

    void DebugPredators()
    {
        Gizmos.color = new Color(0, 0, 0, 0.5f);

        var predators = GetPredatorsInFOVRange();
        foreach (var predator in predators)
        {
            Gizmos.DrawLine(transform.position, predator.transform.position);
        }

        GameObject m = (GameObject)GetComponent<BehaviorTree.Tree>().Root().GetData("predator");
        if (m != null)
        {
            Gizmos.DrawSphere(m.transform.position, 2);
        }
    }

    #endregion
}
