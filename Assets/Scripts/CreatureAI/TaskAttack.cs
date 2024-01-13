using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskAttack : Node
{
    private AgentBehavior _agentBehavior;
    public TaskAttack(AgentBehavior agentBehavior)
    {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate()
    {
        GameObject target = (GameObject)GetData("target");
        if (!_agentBehavior.IsTargetInAttackRange(target)) // target ran away
        {
            state = NodeState.FAILURE;
            return state;
        }

        _agentBehavior.Attack(target);

        if (target.GetComponent<AgentBehavior>().CurrentAgentState == AgentBehavior.AgentState.DEAD)
        {
            ClearData("target");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
