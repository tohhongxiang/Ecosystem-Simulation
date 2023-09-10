using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskMate : Node
{
    private AgentBehavior _agentBehavior;
    public TaskMate(AgentBehavior agentBehavior)
    {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate()
    {
        GameObject target = (GameObject)GetData("mate");
        _agentBehavior.Mate(target);

        if (_agentBehavior.getAgentState() == AgentBehavior.AgentState.DONE_MATING)
        {
            ClearData("mate");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
