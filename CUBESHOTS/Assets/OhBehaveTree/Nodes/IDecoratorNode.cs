using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class IDecoratorNode : INode, IParentNode
	{
		public INode child;

		public void AddNode(INode newNode)
		{
			if (newNode.parent != null)
			{
				newNode.parent.RemoveNode(newNode);
			}

			child = newNode;
			child.parent = this;
		}

		public void RemoveNode(INode removeNode)
		{
			if (removeNode != child)
			{
				Debug.LogError("IDecoratorNode - trying to decouple the wrong parent and child!");
			}
			else
			{
				removeNode.parent = null;
				child = null;
			}
		}

		public abstract INode ChildFinished(NodeState childNodeState);
	}
}