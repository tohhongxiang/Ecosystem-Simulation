using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Inverter : Node
    {
        protected Node node;

        public Inverter(Node node) {
            this.node = node;
        }

        public override NodeState Evaluate() {
            switch (node.Evaluate()) {
                case NodeState.RUNNING:
                    state = NodeState.RUNNING;
                    break;
                case NodeState.SUCCESS:
                    state = NodeState.FAILURE;
                    break;
                case NodeState.FAILURE:
                    state = NodeState.SUCCESS;
                    break;
                default:
                    break;
            }

            return state;
        }
    }

}