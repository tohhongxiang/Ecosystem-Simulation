using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CreatureBehaviorTree : BehaviorTree.Tree
{
    protected override Node SetupTree() {
        AgentBehavior agentBehavior = GetComponent<AgentBehavior>();

        Node root = new Selector(new List<Node>{
            // new Sequence(new List<Node> {
            //     new CheckMateInInteractableRange(agentBehavior),
            //     new TaskMate(agentBehavior),
            // }),
            // new Sequence(new List<Node> {
            //     new CheckCanMate(agentBehavior),
            //     new CheckMateInFOVRange(agentBehavior),
            //     new TaskGoToMate(agentBehavior),
            // }),
            new Sequence(new List<Node> {
                new CheckFoodInInteractableRange(agentBehavior),
                new TaskEat(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckHungerLowerThanThirst(agentBehavior),
                new CheckFoodInFOVRange(agentBehavior),
                new TaskGoToFood(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckWaterInInteractableRange(agentBehavior),
                new TaskDrink(agentBehavior),
            }),
            new Sequence(new List<Node> {
                new CheckThirstLowerThanHunger(agentBehavior),
                new CheckWaterInFOVRange(agentBehavior),
                new TaskGoToWater(agentBehavior),
            }),
            new TaskWander(agentBehavior)
        });

        return root;
    }
}
