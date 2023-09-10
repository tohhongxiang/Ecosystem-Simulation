using System.Collections;
using System.Collections.Generic;
using BehaviorTree;
using UnityEngine;

public class CheckMateInFOVRange : Node
{
    private AgentBehavior _agentBehavior;
    public CheckMateInFOVRange(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }
    
    public override NodeState Evaluate()
    {
        GameObject t = (GameObject)GetData("mate");
        if (t == null)
        { // if there is no current target, check if there are valid targets
            List<GameObject> targetMates = _agentBehavior.GetMatesInFOVRange();
            if (targetMates.Count == 0)
            {
                state = NodeState.FAILURE;
                return state;
            }
            
            parent.parent.SetData("mate", targetMates[0]);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
