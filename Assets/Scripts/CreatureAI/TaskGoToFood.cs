using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskGoToFood : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskGoToFood(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        GameObject g = (GameObject)GetData("target");

        // food is already eaten, or food is being interacted with by something else
        if (g == null || g.layer != LayerMask.NameToLayer(_agentBehavior.foodTag))
        {
            ClearData("target");

            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.CurrentAgentState != AgentBehavior.AgentState.GOING_TO_FOOD)
        {
            _agentBehavior.GoToFood(g);
        }

        state = NodeState.RUNNING;
        return state;
    }
}
