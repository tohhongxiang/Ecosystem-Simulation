
using UnityEngine;

public enum Gender { MALE, FEMALE };

[System.Serializable]
public class AgentStats
{
    [Header("Mutable Stats")]
    public float speed = 1;
    public float size = 1;

    public float hungerDecreaseFactor = 1f;
    public float thirstDecreaseFactor = 1f;
    public float staminaDecreaseFactor = 1f;

    public float gestationDuration = 10f;
    public float durationBetweenPregnancies = 10f;

    public float fovRange = 10;
    public float growIntoAdultDurationSeconds = 30;
    public float expectedAge = 100;
    
    public Gender gender = Gender.MALE;

    // cap mutable stat
    private readonly float maxSpeed = 5.0f;
    private readonly float minSpeed = 0.1f;
    private readonly float maxSize = 3f;
    private readonly float minSize = 0.1f;
    private readonly float maxHungerDecreaseFactor = 100f;
    private readonly float minHungerDecreaseFactor = 0.1f;
    private readonly float maxThirstDecreaseFactor = 100f;
    private readonly float minThirstDecreaseFactor = 0.1f;
    private readonly float maxStaminaDecreaseFactor = 100f;
    private readonly float minStaminaDecreaseFactor = 0.1f;
    private readonly float maxGestationDuration = 1000f;
    private readonly float minGestationDuration = 1f;
    private readonly float maxDurationBetweenPregnancies = 1000f;
    private readonly float minDurationBetweenPregnancies = 1f;
    private readonly float maxFovRange = 100;
    private readonly float minFovRange = 1;
    private readonly float maxGrowIntoAdultDurationSeconds = 1000;
    private readonly float minGrowIntoAdultDurationSeconds = 10;
    private readonly float maxExpectedAge = 100000;
    private readonly float minExpectedAge = 1;

    public AgentStats(AgentStats parent1, AgentStats parent2)
    {
        AgentStats[] parents = { parent1, parent2 };

        // choose from one of the parents
        gender = (Gender)Random.Range(0, System.Enum.GetValues(typeof(Gender)).Length); // random gender

        speed = parents[Random.Range(0, parents.Length)].speed;
        size = parents[Random.Range(0, parents.Length)].size;
        hungerDecreaseFactor = parents[Random.Range(0, parents.Length)].hungerDecreaseFactor;
        thirstDecreaseFactor = parents[Random.Range(0, parents.Length)].thirstDecreaseFactor;
        staminaDecreaseFactor = parents[Random.Range(0, parents.Length)].staminaDecreaseFactor;
        gestationDuration = parents[Random.Range(0, parents.Length)].gestationDuration;
        durationBetweenPregnancies = parents[Random.Range(0, parents.Length)].durationBetweenPregnancies;
        fovRange = parents[Random.Range(0, parents.Length)].fovRange;
        growIntoAdultDurationSeconds = parents[Random.Range(0, parents.Length)].growIntoAdultDurationSeconds;
        expectedAge = parents[Random.Range(0, parents.Length)].expectedAge;

        // random perturbations
        float minPerturbation = 0.75f;
        float maxPerturbation = 1.25f;

        speed *= Random.Range(minPerturbation, maxPerturbation);
        size *= Random.Range(minPerturbation, maxPerturbation);
        // hungerDecreaseFactor *= Random.Range(minPerturbation, maxPerturbation);
        // thirstDecreaseFactor *= Random.Range(minPerturbation, maxPerturbation);
        // staminaDecreaseFactor *= Random.Range(minPerturbation, maxPerturbation);
        // gestationDuration *= Random.Range(minPerturbation, maxPerturbation);
        // durationBetweenPregnancies *= Random.Range(minPerturbation, maxPerturbation);
        // fovRange *= Random.Range(minPerturbation, maxPerturbation);
        // growIntoAdultDurationSeconds *= Random.Range(minPerturbation, maxPerturbation);
        // expectedAge *= Random.Range(minPerturbation, maxPerturbation);

        // clamp
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        size = Mathf.Clamp(size, minSize, maxSize);
        hungerDecreaseFactor = Mathf.Clamp(hungerDecreaseFactor, minHungerDecreaseFactor, maxHungerDecreaseFactor);
        thirstDecreaseFactor = Mathf.Clamp(thirstDecreaseFactor, minThirstDecreaseFactor, maxThirstDecreaseFactor);
        staminaDecreaseFactor = Mathf.Clamp(staminaDecreaseFactor, minStaminaDecreaseFactor, maxStaminaDecreaseFactor);
        gestationDuration = Mathf.Clamp(gestationDuration, minGestationDuration, maxGestationDuration);
        durationBetweenPregnancies = Mathf.Clamp(durationBetweenPregnancies, minDurationBetweenPregnancies, maxDurationBetweenPregnancies);
        fovRange = Mathf.Clamp(fovRange, minFovRange, maxFovRange);
        growIntoAdultDurationSeconds = Mathf.Clamp(growIntoAdultDurationSeconds, minGrowIntoAdultDurationSeconds, maxGrowIntoAdultDurationSeconds);
        expectedAge = Mathf.Clamp(expectedAge, minExpectedAge, maxExpectedAge);
    }
}
