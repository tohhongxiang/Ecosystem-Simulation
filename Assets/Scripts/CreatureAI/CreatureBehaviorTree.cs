using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CreatureBehaviorTree : BehaviorTree.Tree
{
    protected override Node SetupTree() {
        AgentBehavior agentBehavior = GetComponent<AgentBehavior>();

        Node root = new Selector(new List<Node>{
            new Sequence(new List<Node> {
                new CheckFoodInEatRange(agentBehavior),
                new TaskEat(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckFoodInFOVRange(agentBehavior),
                new TaskGoToTarget(agentBehavior),
            }),
            new TaskWander(agentBehavior)
        });

        return root;
    }
}
