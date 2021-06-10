using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that returns Success if any of it's children return Success.
	/// Evaluates all Nodes in random order and returns on first child Success.
	/// </summary>
	public class RandomSelectorNode : ICompositeNode
	{
		private int currentChildIndex;
		private List<int> childrenLeftToRun;


		public RandomSelectorNode()
		{
			nodeType = NodeType.Selector;
			childrenLeftToRun = new List<int>();
		}


		public override INode Init()
		{
			Debug.Log("RandomSelectorNode init");
			nodeState = NodeState.Running;

			childrenLeftToRun.Clear();
			for (int i = 0; i < children.Count; ++i)
			{
				childrenLeftToRun.Add(i);
			}

			currentChildIndex = Random.Range(0, childrenLeftToRun.Count - 1);
			childrenLeftToRun.Remove(currentChildIndex);

			INode next = children[currentChildIndex].Init();
			if (next != null)
				return next;
			return null;
		}

		/// <summary>
		/// Continues through all children randomly until any child reports a success, then this node returns a success.
		/// If all children fail, this returns fail.
		/// </summary>
		/// <returns></returns>
		public override INode ChildFinished(NodeState childNodeState)
		{
			if (childNodeState == NodeState.Success)
			{
				nodeState = NodeState.Success;
				return this; // could just call Exit() here? while loop vs tail-end recursion?
			}


			if (childrenLeftToRun.Count == 0)
			{
				nodeState = NodeState.Failure;
				return this;
			}

			currentChildIndex = childrenLeftToRun[Random.Range(0, childrenLeftToRun.Count - 1)];
			childrenLeftToRun.Remove(currentChildIndex);

			return children[currentChildIndex].Init();
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}