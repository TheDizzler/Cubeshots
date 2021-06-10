using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class ICompositeNode : INode, IParentNode
	{
		public List<INode> children = new List<INode>();

		public void AddNode(INode newNode)
		{
			children.Add(newNode);
			if (newNode.parent != null)
			{
				newNode.parent.RemoveNode(newNode);
			}

			newNode.parent = this;
		}

		public void RemoveNode(INode remove)
		{
			children.Remove(remove);
			if (remove.parent != this)
			{
				Debug.LogError("ICompositeNode - trying to illegally change a nodes parent");
			}
			else
				remove.parent = null;
		}

		public override NodeState Evaluate()
		{
			throw new System.Exception("Composite Nodes should never evaluate!");
		}

		public abstract INode ChildFinished(NodeState childNodeState);
	}
}
