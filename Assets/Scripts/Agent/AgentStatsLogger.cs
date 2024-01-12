using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class AgentStatsLogger : MonoBehaviour
{
    public List<AgentSpawner> agentSpawners = new List<AgentSpawner>();
    public bool saveToCSV = true;
    public float getAverageIntervalSeconds = 10;
    public float writeToCSVIntervalSeconds = 180;

    public static AgentStatsLogger Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private float getAverageIntervalCounter = 0;
    private float writeToCSVIntervalCounter = 0;
    private string startOfExperiment;
    // { [agentSpawner]: { [statistic]: object[] }}
    private Dictionary<string, Dictionary<string, List<object>>> statistics = new Dictionary<string, Dictionary<string, List<object>>>();

    void Start()
    {
        startOfExperiment = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        getAverageIntervalCounter = getAverageIntervalSeconds; // trigger collecting data from t = 0

        foreach (var agentSpawner in agentSpawners)
        {
            statistics.Add(agentSpawner.Name, new Dictionary<string, List<object>>());

            statistics[agentSpawner.Name].Add("population", new List<object>());
            statistics[agentSpawner.Name].Add("time", new List<object>());

            foreach (var prop in typeof(AgentStats).GetFields())
            {
                if (!prop.FieldType.IsNumeric()) continue;

                statistics[agentSpawner.Name].Add(prop.Name, new List<object>());
            }
        }
    }

    void Update()
    {
        writeToCSVIntervalCounter += Time.deltaTime;
        if (writeToCSVIntervalCounter > writeToCSVIntervalSeconds && saveToCSV)
        {
            writeToCSVIntervalCounter = 0;
            WriteStatsToCSV();
            ClearStats();
        }

        getAverageIntervalCounter += Time.deltaTime;
        if (getAverageIntervalCounter > getAverageIntervalSeconds)
        {
            getAverageIntervalCounter = 0;
            UpdateStats();
        }
    }

    void WriteStatsToCSV()
    {
        var finalResultToWrite = new Dictionary<string, List<object>>();
        foreach (var agentSpawner in agentSpawners)
        {
            int rows = 0;
            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.Name])
            {
                rows = element.Value.Count();
                if (finalResultToWrite.ContainsKey(element.Key))
                {
                    finalResultToWrite[element.Key].AddRange(element.Value);
                }
                else
                {
                    finalResultToWrite.Add(element.Key, element.Value);
                }
            }

            string speciesColumnName = "species";
            List<object> species = Enumerable.Repeat(agentSpawner.Name, rows).Cast<object>().ToList();
            if (finalResultToWrite.ContainsKey(speciesColumnName))
            {
                finalResultToWrite[speciesColumnName].AddRange(species);
            }
            else
            {
                finalResultToWrite.Add(speciesColumnName, species);
            }
        }

        WriteFile("Statistics", finalResultToWrite);
    }

    void ClearStats()
    {
        foreach (var agentSpawner in agentSpawners)
        {
            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.Name])
            {
                statistics[agentSpawner.Name][element.Key].Clear();
            }
        }
    }

    private int timer = 0;
    void UpdateStats()
    {
        timer++;
        foreach (var agentSpawner in agentSpawners)
        {
            var childrenAgentBehavior = agentSpawner.Spawner.GetComponentsInChildren<AgentBehavior>();

            int population = childrenAgentBehavior.Count();
            statistics[agentSpawner.Name]["population"].Add(population);
            Debug.Log(string.Format("{0} population: {1}", agentSpawner.Name, population));

            statistics[agentSpawner.Name]["time"].Add(timer);

            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.Name])
            {
                // make sure field exists
                if (typeof(AgentStats).GetField(element.Key) == null)
                {
                    continue;
                }

                if (population == 0)
                {
                    statistics[agentSpawner.Name][element.Key].Add(0);
                    continue;
                }

                var populationStatisticAverage = childrenAgentBehavior
                    .Select(childAgentBehavior => (float)childAgentBehavior.stats.GetType().GetField(element.Key).GetValue(childAgentBehavior.stats))
                    .ToList()
                    .Average();

                statistics[agentSpawner.Name][element.Key].Add(populationStatisticAverage);
            }
        }
    }

    public void WriteFile(string filename, Dictionary<string, List<object>> data)
    {
        var path = "Assets/CSV_OUTPUT/";
        var dir = new DirectoryInfo(path);

        if (!dir.Exists)
        {
            dir.Create();
        }

        string fullFileName = path + filename + "-" + startOfExperiment + ".csv";
        StreamWriter output;

        string[] keys = data.Keys.ToArray();
        if (File.Exists(fullFileName))
        {
            output = File.AppendText(fullFileName);
        }
        else
        {
            output = File.CreateText(fullFileName);
            output.Write(string.Join(",", keys) + "\n");
        }

        for (int i = 0; i < data[keys[0]].Count; i++)
        {
            object[] currentRowData = keys.Select(x => data[x][i].ToString()).ToArray();
            output.Write(string.Join(",", currentRowData) + "\n");
        }

        output.Close();
        Debug.Log("Finished writing to: " + fullFileName);
    }
}

[System.Serializable]
public class AgentSpawner
{
    public string Name;
    public GameObject Spawner;
}