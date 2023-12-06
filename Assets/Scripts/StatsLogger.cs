using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StatsLogger : MonoBehaviour
{
    public float getAverageIntervalSeconds = 10;
    public float writeToCSVIntervalSeconds = 180;
    private float getAverageIntervalCounter = 0;
    private float writeToCSVIntervalCounter = 0;
    private List<float> averageHealth = new List<float>();
    private List<int> childrenCount = new List<int>();

    // Update is called once per frame
    void Update()
    {
        writeToCSVIntervalCounter += Time.deltaTime;
        if (writeToCSVIntervalCounter > writeToCSVIntervalSeconds) {
            WriteFile("health", averageHealth);
            WriteFile("population", childrenCount);
        }

        getAverageIntervalCounter += Time.deltaTime;
        if (getAverageIntervalCounter > getAverageIntervalSeconds) {
            var childrenAgentBehavior = gameObject.GetComponentsInChildren<AgentBehavior>();

            if (childrenAgentBehavior.Length == 0) { // everything is dead
                return;
            }

            float averageMaxHealth = childrenAgentBehavior.Select(childAgentBehavior => childAgentBehavior.stats.maxHealth).ToList().Average();
            averageHealth.Add(averageMaxHealth);
            childrenCount.Add(childrenAgentBehavior.Count());
            getAverageIntervalCounter = 0;
        }
    }

    

    public void WriteFile<T>(string filename, List<T> data) {
		var path = "Assets/CSV_OUTPUT/";
		var dir = new DirectoryInfo(path);

		if (!dir.Exists) {
			dir.Create();
		}

		var output = File.CreateText(path + filename + ".csv");
		foreach (var str in data) {
			output.Write(str.ToString() + "\n");
		}

		output.Close ();
	}
}
