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

        _agentBehavior.Drink();

        if (_agentBehavior.GetAgentState() == AgentBehavior.AgentState.DONE_DRINKING)
        {
            ClearData("water");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
