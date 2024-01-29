using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskEvade : Node
{
    private AgentBehavior _agentBehavior;
    public TaskEvade(AgentBehavior agentBehavior)
    {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate()
    {
        GameObject predator = (GameObject)GetData("predator");

        // forget everything else and run
        ClearData("target");
        ClearData("mate");

        _agentBehavior.Evade(predator);

        if ((_agentBehavior.gameObject.transform.position - predator.transform.position).sqrMagnitude > _agentBehavior.stats.fovRange * _agentBehavior.stats.fovRange)
        {
            ClearData("predator");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
