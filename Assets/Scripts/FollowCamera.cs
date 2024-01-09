using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class FollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, 0);
    private Transform target;
    public List<GameObject> agentSpawners = new List<GameObject>();
    public float changeTargetIntervalSeconds = 5;
    private float internalCounter = 0;

    
    void Start()
    {
        target = ChooseRandomTarget();
        internalCounter = changeTargetIntervalSeconds;
    }

    void Update()
    {
        internalCounter += Time.unscaledDeltaTime;
        if (internalCounter > changeTargetIntervalSeconds) {
            internalCounter = 0;
            Transform oldTarget = target;
            target = ChooseRandomTarget();

            // keep old target if we cannot find a new one
            if (target == null) {
                target = oldTarget;
                return;
            }

            if (oldTarget != null) {
                oldTarget.gameObject.GetComponent<HandleDisplayStats>().enabled = false;
                oldTarget.GetChild(1).gameObject.SetActive(false);
            }
            
            target.gameObject.GetComponent<HandleDisplayStats>().enabled = true;
            target.GetChild(1).gameObject.SetActive(true);
        }

        if (target == null) {
            return;
        }

        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    Transform ChooseRandomTarget() {
        GameObject spawner = agentSpawners[Random.Range(0, agentSpawners.Count)];
        if (spawner.transform.childCount == 0) {
            return null;
        }

        Transform target = spawner.transform.GetChild(Random.Range(0, spawner.transform.childCount));
        return target;
    }
}
