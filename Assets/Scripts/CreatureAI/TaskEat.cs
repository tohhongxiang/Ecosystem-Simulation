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
        GameObject target = (GameObject)GetData("target");
        _agentBehavior.Eat(target);

        if (_agentBehavior.getAgentState() == AgentBehavior.AgentState.DONE_EATING)
        {
            ClearData("target");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
