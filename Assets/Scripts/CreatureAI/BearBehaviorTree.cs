using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class BearBehaviorTree : BehaviorTree.Tree
{
    protected override Node SetupTree() {
        AgentBehavior agentBehavior = GetComponent<AgentBehavior>();

        Node root = new Selector(new List<Node>{
            new Sequence(new List<Node> {
                new CheckIfDead(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckCanMate(agentBehavior),
                new CheckMateInInteractableRange(agentBehavior),
                new TaskMate(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckCanMate(agentBehavior),
                new CheckMateInFOVRange(agentBehavior),
                new TaskGoToMate(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckIfHungry(agentBehavior),
                new CheckFoodInAttackRange(agentBehavior),
                new TaskAttack(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckIfThirsty(agentBehavior),
                new CheckWaterInInteractableRange(agentBehavior),
                new TaskDrink(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckIfHungry(agentBehavior),
                new Inverter(new CheckIfRecovering(agentBehavior)),
                new CheckFoodInFOVRange(agentBehavior),
                new TaskPursueFood(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckIfThirsty(agentBehavior),
                new CheckWaterInFOVRange(agentBehavior),
                new TaskGoToWater(agentBehavior),
            }),
            new TaskWander(agentBehavior)
        });

        return root;
    }
}
