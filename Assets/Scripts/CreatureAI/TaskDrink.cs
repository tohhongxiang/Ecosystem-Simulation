using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskDrink : Node
{
    private AgentBehavior _agentBehavior;
    public TaskDrink(AgentBehavior agentBehavior)
    {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate()
    {
        if (GetData("water") is null) {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 waterPoint = (Vector3)GetData("water");
        _agentBehavior.Drink(waterPoint);

        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.DONE_DRINKING)
        {
            ClearData("water");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
