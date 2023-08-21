using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;

public class CheckFoodInFOVRange : Node
{
    private AgentBehavior _agentBehavior;
    public CheckFoodInFOVRange(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        object t = GetData("target");

        if ((t as Object) == null)
        { // if there is no current target, check if there are valid targets
            List<GameObject> targetFoods = _agentBehavior.FoodInFOVRange();
            if (targetFoods.Count == 0)
            {
                state = NodeState.FAILURE;
                return state;
            }
            
            parent.parent.SetData("target", targetFoods[0]);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
