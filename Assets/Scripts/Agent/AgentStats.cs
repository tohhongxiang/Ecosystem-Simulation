
using UnityEngine;

public enum Gender { MALE, FEMALE };

[System.Serializable]
public class AgentStats
{
    [Header("Mutable Stats")]
    public float speed = 1;
    public float maxHealth = 100;
    public float maxHunger = 100;
    public float maxThirst = 100;
    public float maxStamina = 100;
    public float maxReproductiveSatisfaction = 100;
    public float fovRange = 10;
    public float matingCooldownSeconds = 30;
    public float growIntoAdultDurationSeconds = 30;
    public float expectedAge = 100;
    public Gender gender = Gender.MALE;

    // cap mutable stat
    private readonly float maxSpeed = 5.0f;
    private readonly float minSpeed = 0.1f;
    private readonly float maxMaxHealth = 1000;
    private readonly float minMaxHealth = 10;
    private readonly float maxMaxHunger = 1000;
    private readonly float minMaxHunger = 10;
    private readonly float maxMaxThirst = 1000;
    private readonly float minMaxThirst = 10;
    private readonly float maxMaxStamina = 1000;
    private readonly float minMaxStamina = 1;
    private readonly float maxMaxReproductiveSatisfaction = 1000;
    private readonly float minMaxReproductiveSatisfaction = 1;
    private readonly float maxFovRange = 100;
    private readonly float minFovRange = 1;
    private readonly float maxMatingCooldownSeconds = 1000;
    private readonly float minMatingCooldownSeconds = 10;
    private readonly float maxGrowIntoAdultDurationSeconds = 1000;
    private readonly float minGrowIntoAdultDurationSeconds = 10;
    private readonly float maxExpectedAge = 100000;
    private readonly float minExpectedAge = 1;

    public AgentStats() {}

    public AgentStats(float speed, float maxHealth, float maxHunger, float maxThirst, float maxStamina, float maxReproductiveSatisfaction, float fovRange, float matingCooldownSeconds, float growIntoAdultDurationSeconds, Gender gender, float expectedAge)
    {
        this.speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        this.maxHealth = Mathf.Clamp(maxHealth, minMaxHealth, maxMaxHealth);
        this.maxHunger = Mathf.Clamp(maxHunger, minMaxHunger, maxMaxHunger);
        this.maxThirst = Mathf.Clamp(maxThirst, minMaxThirst, maxMaxThirst);
        this.maxStamina = Mathf.Clamp(maxStamina, minMaxStamina, maxMaxStamina);
        this.maxReproductiveSatisfaction = Mathf.Clamp(maxReproductiveSatisfaction, minMaxReproductiveSatisfaction, maxMaxReproductiveSatisfaction);
        this.fovRange = Mathf.Clamp(fovRange, minFovRange, maxFovRange);
        this.matingCooldownSeconds = Mathf.Clamp(matingCooldownSeconds, minMatingCooldownSeconds, maxMatingCooldownSeconds);
        this.growIntoAdultDurationSeconds = Mathf.Clamp(growIntoAdultDurationSeconds, minGrowIntoAdultDurationSeconds, maxGrowIntoAdultDurationSeconds);
        this.gender = gender;
        this.expectedAge = Mathf.Clamp(expectedAge, minExpectedAge, maxExpectedAge);
    }

    public AgentStats(AgentStats parent1, AgentStats parent2)
    {
        AgentStats[] parents = { parent1, parent2 };

        // choose from one of the parents
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length); // random gender

        speed = parents[Random.Range(0, parents.Length)].speed;
        maxHealth = parents[Random.Range(0, parents.Length)].maxHealth;
        maxHunger = parents[Random.Range(0, parents.Length)].maxHunger;
        maxThirst = parents[Random.Range(0, parents.Length)].maxThirst;
        maxStamina = parents[Random.Range(0, parents.Length)].maxStamina;
        maxReproductiveSatisfaction = parents[Random.Range(0, parents.Length)].maxReproductiveSatisfaction;
        fovRange = parents[Random.Range(0, parents.Length)].fovRange;
        matingCooldownSeconds = parents[Random.Range(0, parents.Length)].matingCooldownSeconds;
        growIntoAdultDurationSeconds = parents[Random.Range(0, parents.Length)].growIntoAdultDurationSeconds;
        expectedAge = parents[Random.Range(0, parents.Length)].expectedAge;

        // random perturbations
        float minPerturbation = 0.75f;
        float maxPerturbation = 1.25f;

        speed *= Random.Range(minPerturbation, maxPerturbation);
        maxHealth *= Random.Range(minPerturbation, maxPerturbation);
        maxHunger *= Random.Range(minPerturbation, maxPerturbation);
        maxThirst *= Random.Range(minPerturbation, maxPerturbation);
        maxStamina *= Random.Range(minPerturbation, maxPerturbation);
        maxReproductiveSatisfaction *= Random.Range(minPerturbation, maxPerturbation);
        fovRange *= Random.Range(minPerturbation, maxPerturbation);
        matingCooldownSeconds *= Random.Range(minPerturbation, maxPerturbation);
        growIntoAdultDurationSeconds *= Random.Range(minPerturbation, maxPerturbation);
        expectedAge *= Random.Range(minPerturbation, maxPerturbation);

        // clamp
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        maxHealth = Mathf.Clamp(maxHealth, minMaxHealth, maxMaxHealth);
        maxHunger = Mathf.Clamp(maxHunger, minMaxHunger, maxMaxHunger);
        maxThirst = Mathf.Clamp(maxThirst, minMaxThirst, maxMaxThirst);
        maxStamina = Mathf.Clamp(maxStamina, minMaxStamina, maxMaxStamina);
        maxReproductiveSatisfaction = Mathf.Clamp(maxReproductiveSatisfaction, minMaxReproductiveSatisfaction, maxMaxReproductiveSatisfaction);
        fovRange = Mathf.Clamp(fovRange, minFovRange, maxFovRange);
        matingCooldownSeconds = Mathf.Clamp(matingCooldownSeconds, minMatingCooldownSeconds, maxMatingCooldownSeconds);
        growIntoAdultDurationSeconds = Mathf.Clamp(growIntoAdultDurationSeconds, minGrowIntoAdultDurationSeconds, maxGrowIntoAdultDurationSeconds);
        expectedAge = Mathf.Clamp(expectedAge, minExpectedAge, maxExpectedAge);
    }
}
