using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskEat : Node
{
    private AgentBehavior _agentBehavior;
    public TaskEat(AgentBehavior agentBehavior)
    {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate()
    {
        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.DONE_EATING)
        {
            ClearData("target");
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.EATING) {
            state = NodeState.RUNNING;
            return state;
        }

        GameObject target = (GameObject)GetData("target");
        _agentBehavior.Eat(target);

        state = NodeState.RUNNING;
        return state;
    }
}
