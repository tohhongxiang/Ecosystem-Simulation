using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class StatsLogger : MonoBehaviour
{
    public float getAverageIntervalSeconds = 10;
    public float writeToCSVIntervalSeconds = 180;
    public string fileNamePrefix = "";

    private float getAverageIntervalCounter = 0;
    private float writeToCSVIntervalCounter = 0;

    private List<object> populationCount = new List<object>();
    private List<object> populationSpeed = new List<object>();
    private List<object> populationHealth = new List<object>();
    private List<object> populationHunger = new List<object>();
    private List<object> populationThirst = new List<object>();
    private List<object> populationStamina = new List<object>();
    private List<object> populationFovRange = new List<object>();
    private List<object> populationMatingCooldownSeconds = new List<object>();
    private List<object> populationReproductionTimeSeconds = new List<object>();
    private List<object> populationGrowIntoAdultDurationSeconds = new List<object>();

    private string startOfExperiment;

    void Start() {
        startOfExperiment = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ffff");
    }

    // Update is called once per frame
    void Update()
    {
        writeToCSVIntervalCounter += Time.deltaTime;
        if (writeToCSVIntervalCounter > writeToCSVIntervalSeconds) {
            writeToCSVIntervalCounter = 0;
            Debug.Log("Write to csv");

            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>
            {
                { "population", populationCount },
                { "speed", populationSpeed },
                { "health", populationHealth },
                { "hunger", populationHunger },
                { "thirst", populationThirst },
                { "stamina", populationStamina },
                { "fovRange", populationFovRange },
                { "mating_cooldown_seconds", populationMatingCooldownSeconds },
                { "reproduction_time_seconds", populationReproductionTimeSeconds },
                { "grow_into_adult_duration_seconds", populationGrowIntoAdultDurationSeconds },
            };

            string fileName = fileNamePrefix.Length > 0 ? "Statistics-" + fileNamePrefix : "Statistics";
            WriteFile(fileName, data);

            // clear out all current data
            populationCount.Clear();
            populationSpeed.Clear();
            populationHealth.Clear();
            populationHunger.Clear();
            populationThirst.Clear();
            populationStamina.Clear();
            populationFovRange.Clear();
            populationMatingCooldownSeconds.Clear();
            populationReproductionTimeSeconds.Clear();
            populationGrowIntoAdultDurationSeconds.Clear();
        }

        getAverageIntervalCounter += Time.deltaTime;
        if (getAverageIntervalCounter > getAverageIntervalSeconds) {
            var childrenAgentBehavior = gameObject.GetComponentsInChildren<AgentBehavior>();

            if (childrenAgentBehavior.Length == 0) { // everything is dead
                return;
            }

            populationCount.Add(childrenAgentBehavior.Count());

            float averageSpeed = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.speed).ToList().Average();
            populationSpeed.Add(averageSpeed);

            float averageMaxHealth = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxHealth).ToList().Average();
            populationHealth.Add(averageMaxHealth);

            float averageMaxHunger = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxHunger).ToList().Average();
            populationHunger.Add(averageMaxHunger);

            float averageMaxThirst = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxThirst).ToList().Average();
            populationThirst.Add(averageMaxThirst);

            float averageMaxStamina = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxStamina).ToList().Average();
            populationStamina.Add(averageMaxStamina);

            float averageFovRange = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.fovRange).ToList().Average();
            populationFovRange.Add(averageFovRange);

            float averageMatingCooldownSeconds = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.matingCooldownSeconds).ToList().Average();
            populationMatingCooldownSeconds.Add(averageMatingCooldownSeconds);

            float averageReproductionTimeSeconds = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.reproductionTimeSeconds).ToList().Average();
            populationReproductionTimeSeconds.Add(averageReproductionTimeSeconds);

            float averageGrowIntoAdultDurationSeconds = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.growIntoAdultDurationSeconds).ToList().Average();
            populationGrowIntoAdultDurationSeconds.Add(averageGrowIntoAdultDurationSeconds);

            getAverageIntervalCounter = 0;
        }
    }

    public void WriteFile(string filename, Dictionary<string, List<object>> data) {
		var path = "Assets/CSV_OUTPUT/";
		var dir = new DirectoryInfo(path);

		if (!dir.Exists) {
			dir.Create();
		}

        string fullFileName = path + filename + "-" + startOfExperiment + ".csv";
        StreamWriter output;

        string[] keys = data.Keys.ToArray();
        if (File.Exists(fullFileName)) {
            output = File.AppendText(fullFileName);
        } else {
            output = File.CreateText(fullFileName);
            output.Write(string.Join(",", keys) + "\n");
        }

        for (int i = 0; i < data[keys[0]].Count; i++) {
            object[] currentRowData = keys.Select(x => data[x][i].ToString()).ToArray();
            output.Write(string.Join(",", currentRowData) + "\n");
        }

		output.Close();
	}
}
