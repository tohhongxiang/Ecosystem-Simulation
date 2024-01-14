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

        if (t != null)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        // if there is no current target, check if there are valid targets
        List<GameObject> targetMates = _agentBehavior.GetMatesInFOVRange();
        if (targetMates.Count == 0)
        {
            state = NodeState.FAILURE;
            return state;
        }

        foreach (var mate in targetMates)
        {
            GameObject mateCandidate = (GameObject)mate.GetComponent<BehaviorTree.Tree>().Root().GetData("mate");
            if (mateCandidate != null)
            {
                continue;
            }

            parent.parent.SetData("mate", mate);
            mate.GetComponent<BehaviorTree.Tree>().Root().SetData("mate", _agentBehavior.gameObject);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
