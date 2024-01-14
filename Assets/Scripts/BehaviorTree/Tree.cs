using UnityEngine;

namespace BehaviorTree
{
    public abstract class Tree : MonoBehaviour
    {
        private Node _root = null;
        public Node Root() {
            return _root;
        }
        
        protected void Start()
        {
            _root = SetupTree();
        }

        void Update()
        {
            if (_root != null) {
                _root.Evaluate();
            }
        }

        protected abstract Node SetupTree();
    }
}

