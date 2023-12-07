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

    private float getAverageIntervalCounter = 0;
    private float writeToCSVIntervalCounter = 0;

    private List<object> populationHealth = new List<object>();
    private List<object> populationCount = new List<object>();
    private List<object> populationHealthDecayRate = new List<object>();
    private List<object> populationMatingCooldownSeconds = new List<object>();
    private List<object> populationReproductionTimeSeconds = new List<object>();
    private List<object> populationGrowIntoAdultDurationSeconds = new List<object>();

    private String startOfExperiment;

    void Start() {
        startOfExperiment = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ffff");
    }

    // Update is called once per frame
    void Update()
    {
        writeToCSVIntervalCounter += Time.deltaTime;
        if (writeToCSVIntervalCounter > writeToCSVIntervalSeconds) {
            Debug.Log("Write to csv");

            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>
            {
                { "population", populationCount },
                { "health", populationHealth },
                { "health_decay_rate", populationHealthDecayRate },
                { "mating_cooldown_seconds", populationMatingCooldownSeconds },
                { "reproduction_time_seconds", populationReproductionTimeSeconds },
                { "grow_into_adult_duration_seconds", populationGrowIntoAdultDurationSeconds },
            };

            WriteFile("Statistics", data);
            writeToCSVIntervalCounter = 0;

            // clear out all current data
            populationCount.Clear();
            populationHealth.Clear();
            populationHealthDecayRate.Clear();
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

            float averageMaxHealth = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxHealth).ToList().Average();
            populationHealth.Add(averageMaxHealth);

            populationCount.Add(childrenAgentBehavior.Count());

            float averageHealthDecayRate = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.healthDecayRate).ToList().Average();
            populationHealthDecayRate.Add(averageHealthDecayRate);

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
            output.Write(string.Join(", ", keys) + "\n");
        }

        for (int i = 0; i < data[keys[0]].Count; i++) {
            object[] currentRowData = keys.Select(x => data[x][i].ToString()).ToArray();
            output.Write(string.Join(", ", currentRowData) + "\n");
        }

		output.Close();
	}
}
