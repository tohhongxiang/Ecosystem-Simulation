using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Unity.VisualScripting;
using UnityEditor;
using TMPro;

public class AgentStatsLogger : MonoBehaviour
{
    public List<TerrainObjectWithCountGenerator> agentSpawners = new List<TerrainObjectWithCountGenerator>();

    public bool saveToCSV = true;
    public float getAverageIntervalSeconds = 10;
    public float writeToCSVIntervalSeconds = 180;

    public GameObject agentPopulationCanvas;
    private Dictionary<string, TMP_Text> agentToText = new Dictionary<string, TMP_Text>();
    private TMP_Text timeText;
    private readonly int defaultFontSize = 20;

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
    private bool simulationAlreadyStarted = false;
    private string startOfExperiment;

    // { [agentSpawner]: { [statistic]: object[] }}
    private Dictionary<string, Dictionary<string, List<object>>> statistics = new Dictionary<string, Dictionary<string, List<object>>>();

    private Dictionary<string, Dictionary<string, int>> eventCounts = new Dictionary<string, Dictionary<string, int>>();
    private readonly string[] eventNames = { "deathsByAge", "deathsByHunger", "deathsByThirst", "deathsByHunt", "deathsTotal", "births" };
    public void AddCountToEvent(string agentSpawnerName, string eventName, int count)
    {
        eventCounts[agentSpawnerName][eventName] += count;
    }

    void Start()
    {
        startOfExperiment = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        getAverageIntervalCounter = getAverageIntervalSeconds; // trigger collecting data from t = 0

        // remove agent spawners which do not spawn anything
        agentSpawners = agentSpawners.Where(agentSpawner => agentSpawner.count > 0).ToList();

        foreach (var agentSpawner in agentSpawners)
        {
            // set up events to listen to
            eventCounts.Add(agentSpawner.generatorName, new Dictionary<string, int>());
            foreach (var eventName in eventNames)
            {
                eventCounts[agentSpawner.generatorName].Add(eventName, 0);
            }

            // setup traits to track
            statistics.Add(agentSpawner.generatorName, new Dictionary<string, List<object>>());

            statistics[agentSpawner.generatorName].Add("population", new List<object>());
            statistics[agentSpawner.generatorName].Add("time", new List<object>());

            foreach (var eventName in eventNames)
            {
                statistics[agentSpawner.generatorName].Add(eventName, new List<object>());
            }

            foreach (var prop in typeof(AgentStats).GetFields())
            {
                if (!prop.FieldType.IsNumeric()) continue;

                statistics[agentSpawner.generatorName].Add(prop.Name, new List<object>());
            }

            // setup text display for population
            TextMeshProUGUI textGameObject = createTextElement(agentSpawner.generatorName, "");
            textGameObject.transform.SetParent(agentPopulationCanvas.transform, false);
            agentToText[agentSpawner.generatorName] = textGameObject;
            UpdatePopulationDisplay(agentSpawner.generatorName, agentSpawner.count);
        }

        TextMeshProUGUI timeTextElement = createTextElement("Time", "");
        timeTextElement.transform.SetParent(agentPopulationCanvas.transform, false);
        timeText = timeTextElement;
        UpdateTimerText();

        StartCoroutine(WaitForStart());
    }

    TextMeshProUGUI createTextElement(string name, string text)
    {
        GameObject textGameObject = new GameObject(name);
        var textComponent = textGameObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = defaultFontSize;
        textComponent.alignment = TextAlignmentOptions.Right;

        return textComponent;
    }

    IEnumerator WaitForStart()
    {
        yield return new WaitForSeconds(1);

        simulationAlreadyStarted = true;
    }

    void Update()
    {
        if (!simulationAlreadyStarted)
        {
            return;
        }

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

        bool simulationEnded = true;
        foreach (var agentSpawner in agentSpawners)
        {
            if (agentSpawner.gameObject.transform.childCount != 0)
            {
                simulationEnded = false;
            }
        }

        if (simulationEnded)
        {
            UpdateStats();
            WriteStatsToCSV();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }

    void WriteStatsToCSV()
    {
        var finalResultToWrite = new Dictionary<string, List<object>>();
        foreach (var agentSpawner in agentSpawners)
        {
            int rows = 0;
            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.generatorName])
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
            List<object> species = Enumerable.Repeat(agentSpawner.generatorName, rows).Cast<object>().ToList();
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
            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.generatorName])
            {
                statistics[agentSpawner.generatorName][element.Key].Clear();
            }
        }
    }

    private int timer = 0;
    void UpdateStats()
    {
        timer += (int)getAverageIntervalSeconds;
        UpdateTimerText();

        foreach (var agentSpawner in agentSpawners)
        {
            if (agentSpawner.count == 0)
            {
                continue;
            }

            statistics[agentSpawner.generatorName]["time"].Add(timer);

            int population = agentSpawner.gameObject.transform.childCount;
            statistics[agentSpawner.generatorName]["population"].Add(population);
            UpdatePopulationDisplay(agentSpawner.generatorName, population);

            Debug.Log(string.Format("{0} population: {1}", agentSpawner.generatorName, population));

            var childrenAgentBehavior = agentSpawner.GetComponentsInChildren<AgentBehavior>();
            foreach (KeyValuePair<string, List<object>> element in statistics[agentSpawner.generatorName])
            {
                // make sure current key is something that exists within agentStats
                if (typeof(AgentStats).GetField(element.Key) == null)
                {
                    continue;
                }

                // if the object we are trying to log does not have agent stats, we just add a 0
                if (childrenAgentBehavior.Count() == 0)
                {
                    statistics[agentSpawner.generatorName][element.Key].Add(0);
                    continue;
                }

                var populationStatisticAverage = childrenAgentBehavior
                    .Select(childAgentBehavior => (float)childAgentBehavior.stats.GetType().GetField(element.Key).GetValue(childAgentBehavior.stats))
                    .ToList()
                    .Average();

                statistics[agentSpawner.generatorName][element.Key].Add(populationStatisticAverage);
            }

            // create a temporary new dictionary to prevent `InvalidOperationException: Collection was modified;`
            Dictionary<string, int> eventCountsToAdd = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> element in eventCounts[agentSpawner.generatorName])
            {
                eventCountsToAdd.Add(element.Key, element.Value);
            }

            foreach (KeyValuePair<string, int> element in eventCountsToAdd)
            {
                statistics[agentSpawner.generatorName][element.Key].Add(element.Value);
                eventCounts[agentSpawner.generatorName][element.Key] -= element.Value;
            }
        }
    }

    void WriteFile(string filename, Dictionary<string, List<object>> data)
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

    void UpdatePopulationDisplay(string name, int population)
    {
        agentToText[name].text = string.Format("{0}: {1}", name, population);
    }

    void UpdateTimerText()
    {
        if (timeText != null)
        {
            timeText.text = string.Format("Time: {0}", timer);
        }
    }
}
