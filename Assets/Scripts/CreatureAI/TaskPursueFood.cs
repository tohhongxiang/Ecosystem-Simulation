using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskPursueFood : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskPursueFood(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate() {
        if (_agentBehavior.CurrentAgentState == AgentBehavior.AgentState.ATTACKING) { // if he is currently attacking he cannot pursue
            state = NodeState.FAILURE;
            return state;
        }

        GameObject g = (GameObject)GetData("target");

        if (g == null || _agentBehavior.IsRecovering) { // target does not exist anymore, or agent ran out of stamina chasing
            ClearData("target");

            state = NodeState.FAILURE;
            return state;
        }

        ClearData("water");
        ClearData("mate");
        _agentBehavior.Pursue(g);

        state = NodeState.RUNNING;
        return state;
    }
}
