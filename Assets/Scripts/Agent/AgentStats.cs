
using UnityEngine;

public enum Gender { MALE, FEMALE };

[System.Serializable]
public class AgentStats
{
    [Header("Mutable Stats")]
    public float speed = 1;
    public float size = 1;

    [Header("Non-mutable Stats")]
    public float hungerDecreaseFactor = 1f;
    public float thirstDecreaseFactor = 1f;
    public float staminaDecreaseFactor = 1f;
    public float staminaIncreaseFactor = 1f;

    public float gestationDuration = 10f;
    public float durationBetweenPregnancies = 10f;

    public float fovRange = 10;
    public float growIntoAdultDurationSeconds = 30;
    public float expectedAge = 100;
    public float averageNumberOfChildren = 1;

    public Gender gender = Gender.MALE;

    // cap mutable stat
    public static readonly float maxSpeed = 5.0f;
    public static readonly float minSpeed = 0.1f;
    public static readonly float maxSize = 3f;
    public static readonly float minSize = 0.1f;

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
        averageNumberOfChildren = parents[Random.Range(0, parents.Length)].averageNumberOfChildren;

        // random perturbations
        float minPerturbation = 0.75f;
        float maxPerturbation = 1.25f;

        speed *= Random.Range(minPerturbation, maxPerturbation);
        size *= Random.Range(minPerturbation, maxPerturbation);

        // clamp
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        size = Mathf.Clamp(size, minSize, maxSize);
    }
}
