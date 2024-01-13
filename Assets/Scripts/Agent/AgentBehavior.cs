using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Pathfinding;
using UnityEngine.Rendering.Universal;
using UnityEngine.Animations;
using Unity.VisualScripting;
using Mono.Cecil.Cil;

[RequireComponent(typeof(Animator), typeof(LocomotionSimpleAgent), typeof(IAstarAI))]
public class AgentBehavior : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // used to instantiate children

    [SerializeField] public string foodTag = "";
    [SerializeField] public string predatorTag = "";

    [Header("Stats")]
    public AgentStats stats;

    // internal state
    public float Health { get; private set; }

    public float Hunger { get; private set; }
    private readonly float hungerThresholdPercentage = 0.8f;
    public bool IsHungry()
    {
        return Hunger <= hungerThresholdPercentage * stats.maxHunger;
    }

    public float Thirst { get; private set; }
    private readonly float thirstThresholdPercentage = 0.8f;
    public bool IsThirsty()
    {
        return Thirst <= thirstThresholdPercentage * stats.maxThirst;
    }

    public float Stamina { get; private set; } = 0;
    public bool IsRecovering { get; private set; } = false;
    private readonly float staminaRecoveryThreshold = 0.9f;

    public float ReproductiveSatisfaction { get; private set; } = 0;

    public float Age { get; private set; } = 0;

    public enum AgentState
    {
        GOING_TO_FOOD, EATING, DONE_EATING,
        GOING_TO_WATER, DRINKING, DONE_DRINKING,
        GOING_TO_MATE, MATING, DONE_MATING,
        ATTACKING, DONE_ATTACKING,
        WANDERING, RUNNING, DEAD
    };
    public AgentState CurrentAgentState { get; private set; } = AgentState.WANDERING;

    private bool isChild = false;
    private float childCounter = 0;
    private const float childScale = 0.5f;

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

        Health = stats.maxHealth;
        Hunger = stats.maxHunger;
        Thirst = stats.maxThirst;
        Stamina = stats.maxStamina;
        ReproductiveSatisfaction = stats.maxReproductiveSatisfaction;
    }

    void Update()
    {
        UpdateStats();
        HandleGrowIntoAdultUpdate();
        HandleAge();
    }

    private void UpdateStats()
    {
        // decrease hunger and thirst all the time, stopping at 0
        Hunger = Mathf.Max(Hunger - Time.deltaTime, 0);
        Thirst = Mathf.Max(Thirst - Time.deltaTime, 0);

        ReproductiveSatisfaction = Mathf.Max(ReproductiveSatisfaction - Time.deltaTime, 0);

        if (CurrentAgentState == AgentState.RUNNING && agent.velocity.magnitude > 0.3f)
        {
            Stamina = Mathf.Max(Stamina - Time.deltaTime, 0);
            if (Stamina == 0 && !IsRecovering)
            {
                IsRecovering = true;
                StartCoroutine(HandleCheckIfRecovered());
            }
        }
        else
        {
            Stamina = Mathf.Min(Stamina + Time.deltaTime, stats.maxStamina);
        }

        if (Hunger <= 0 || Thirst <= 0)
        {
            Health = Mathf.Max(0, Health - Time.deltaTime);
        }

        if (Health <= 0)
        {
            Die();
        }
    }

    IEnumerator HandleCheckIfRecovered()
    {
        while (Stamina < staminaRecoveryThreshold * stats.maxStamina)
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

        float size = Mathf.Lerp(childScale, 1, progressToAdult);
        transform.localScale = new Vector3(size, size, size);

        if (childCounter >= stats.growIntoAdultDurationSeconds)
        {
            isChild = false;
            transform.localScale = new Vector3(1, 1, 1);
            childCounter = 0;
        }
    }

    private float checkDeathCounter = 0;
    private float checkDeathInterval = 30;
    private float probabilityOfDyingWhenBorn = 0.01f;
    private float probabilityOfDyingWhenAtExpectedAge = 0.5f;
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
                Die();
            }
        }

    }

    #region Handle Dying

    private void Die()
    {
        StopAllCoroutines();
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
        if (!agent.pathPending && (agent.reachedEndOfPath || !agent.hasPath))
        {
            agent.destination = RandomPoint();
            agent.SearchPath();
        }
    }

    private Vector3 RandomPoint()
    {
        Vector3 point = Random.insideUnitSphere * stats.fovRange;

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

        Hunger = stats.maxHunger;
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
        Thirst = stats.maxThirst;
        CurrentAgentState = AgentState.DONE_DRINKING;
        agent.isStopped = false;
    }

    public void BlacklistWaterPoint(Vector3 target)
    {
        WaterGenerator.Instance.ClearWaterPoint(target);
    }

    #endregion

    #region Handle Mating
    public bool IsTargetInReproduceRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 6;
    }

    public void GoToMate(GameObject mate)
    {
        CurrentAgentState = AgentState.GOING_TO_MATE;
        locomotionSimpleAgent.Seek(mate.transform.position);

        mate.GetComponent<LocomotionSimpleAgent>().Seek(transform.position);
    }

    private HashSet<GameObject> unimpressedFemales = new HashSet<GameObject>();
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

            if (unimpressedFemales.Contains(collider.gameObject))
            {
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

        AgentBehavior partnerAgentBehavior = mate.GetComponent<AgentBehavior>();
        if (stats.gender == Gender.MALE)
        {
            bool success = partnerAgentBehavior.RequestMate(stats);
            if (success)
            {
                StartCoroutine(HandleMating(mate));
                StartCoroutine(partnerAgentBehavior.HandleMating(mate));
            }
            else
            {
                AddUnimpressed(mate);
                partnerAgentBehavior.AddUnimpressed(gameObject);
                StartCoroutine(ForgetAboutMate(mate));
            }
        }
    }

    public void AddUnimpressed(GameObject mate)
    {
        unimpressedFemales.Add(mate);
    }

    public bool IsUnimpressed(GameObject mate)
    {
        return unimpressedFemales.Contains(mate);
    }

    float forgetTimeSeconds = 30;
    IEnumerator ForgetAboutMate(GameObject mate)
    {
        yield return new WaitForSeconds(forgetTimeSeconds);
        unimpressedFemales.Remove(mate);
    }

    public bool RequestMate(AgentStats mateStats)
    {
        // more likely to mate with worse mates if agent has not mated for a while
        float chance = 1 - ReproductiveSatisfaction / stats.maxReproductiveSatisfaction;

        chance += (mateStats.speed - stats.speed) / stats.speed;
        chance += (mateStats.maxHealth - stats.maxHealth) / stats.maxHealth;
        chance += (mateStats.maxHunger - stats.maxHunger) / stats.maxHunger;
        chance += (mateStats.maxThirst - stats.maxThirst) / stats.maxThirst;
        chance += (mateStats.maxStamina - stats.maxStamina) / stats.maxStamina;

        chance = Mathf.Clamp01(chance);

        return RollForChance(chance);
    }

    private bool RollForChance(float chance)
    {
        return chance > Random.Range(0f, 1f);
    }

    public bool CanMate()
    {
        return !isChild && ReproductiveSatisfaction / stats.maxReproductiveSatisfaction <= Thirst / stats.maxThirst && ReproductiveSatisfaction / stats.maxReproductiveSatisfaction <= Hunger / stats.maxHunger;
    }

    private float reproductionTimeSeconds = 5;
    IEnumerator HandleMating(GameObject mate)
    {
        if (mate == null)
        {
            yield break;
        }

        agent.isStopped = true;
        CurrentAgentState = AgentState.MATING;
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

        animator.SetBool("isMating", false);

        if (mate == null)
        {
            yield break;
        }

        ReproductiveSatisfaction = stats.maxReproductiveSatisfaction;

        CurrentAgentState = AgentState.DONE_MATING;

        if (stats.gender == Gender.FEMALE) // make only the female spawn the child
        {
            GameObject child = Instantiate(prefab, gameObject.transform.parent);
            child.tag = "Untagged";

            AgentBehavior childAgentBehavior = child.GetComponent<AgentBehavior>();
            childAgentBehavior.isChild = true;
            childAgentBehavior.stats = new AgentStats(mate.GetComponent<AgentBehavior>().stats, stats);

            // disable showing stats
            child.GetComponent<HandleDisplayStats>().enabled = false;
            child.transform.GetChild(1).gameObject.SetActive(false);
        }

        agent.isStopped = false;
    }

    #endregion

    #region Handle Predators

    public bool IsTargetInAttackRange(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position) <= agent.radius * 4;
    }

    public void Pursue(GameObject target)
    {
        CurrentAgentState = AgentState.RUNNING;
        locomotionSimpleAgent.Seek(target.transform.position);
    }

    public void Evade(GameObject target)
    {
        CurrentAgentState = AgentState.RUNNING;
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
            Hunger = stats.maxHunger;
        }

        animator.SetBool("isAttacking", false);
        CurrentAgentState = AgentState.DONE_ATTACKING;
        agent.isStopped = false;
    }

    public void Kill()
    {
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
        // DebugFood();
        // DebugWater();
        // DebugDestination();
        // DebugMating();
        // DebugEvade();

        // if (foodTag == "Deer")
        // {
        //     DebugFood();
        // } else {
        //     DebugPredators();
        // }
    }

    void DebugEvade()
    {
        Gizmos.DrawCube(agent.destination, Vector3.one * 3);
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

            Gizmos.color = new Color(1, 1, 1, 0.5f);
            foreach (var waterPoint in waterPoints)
            {
                Gizmos.DrawCube(waterPoint, new Vector3(1, 1, 1));
            }
        }

    }

    void DebugMating()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, agent.radius * 6);

        var mates = GetMatesInFOVRange();
        foreach (var mate in mates)
        {
            Gizmos.DrawLine(transform.position, mate.transform.position);
        }

        Gizmos.color = new Color(1, 1, 1, 0.5f);
        foreach (var unimpressedFemale in unimpressedFemales)
        {
            Gizmos.DrawCube(unimpressedFemale.transform.position, Vector3.one * 3);
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

    void DebugDestination()
    {
        var destination = agent.destination;
        Gizmos.color = new Color(.5f, 1, .4f);
        Gizmos.DrawLine(transform.position, destination);
        Gizmos.DrawSphere(destination, 2);
    }
    #endregion
}
