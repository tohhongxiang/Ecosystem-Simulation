using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Pathfinding.RVO.Sampled;

public class TaskGoToWater : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskGoToWater(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        Vector3 waterPoint = (Vector3)GetData("water");
        if (waterPoint == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.CurrentAgentState != AgentBehavior.AgentState.GOING_TO_WATER)
        {
            _agentBehavior.GoToWater(waterPoint);
        }

        state = NodeState.RUNNING;
        return state;
    }
}
