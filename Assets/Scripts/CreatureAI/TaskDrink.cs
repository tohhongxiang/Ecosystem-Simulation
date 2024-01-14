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
        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.DONE_DRINKING)
        {
            ClearData("water");
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.DRINKING) {
            state = NodeState.RUNNING;
            return state;
        }

        Vector3 waterPoint = (Vector3)GetData("water");
        _agentBehavior.Drink(waterPoint);

        state = NodeState.RUNNING;
        return state;
    }
}
