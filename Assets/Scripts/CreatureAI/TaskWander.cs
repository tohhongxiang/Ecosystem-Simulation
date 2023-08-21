using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using BehaviorTree;

public class TaskWander : Node
{
    private AgentBehavior _agentBehavior;

    public TaskWander(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        _agentBehavior.Wander();
        
        state = NodeState.RUNNING;
        return state;
    }
}
