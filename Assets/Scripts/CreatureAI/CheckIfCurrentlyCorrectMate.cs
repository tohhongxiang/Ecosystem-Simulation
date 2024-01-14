using BehaviorTree;
using UnityEngine;

public class CheckIfCurrentlyCorrectMate : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfCurrentlyCorrectMate(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        GameObject mate = (GameObject)GetData("mate");
        if (mate == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        GameObject mateMate = (GameObject)mate.GetComponent<BehaviorTree.Tree>().Root().GetData("mate");
        if (mateMate != _agentBehavior.gameObject)
        {
            ClearData("mate");

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
