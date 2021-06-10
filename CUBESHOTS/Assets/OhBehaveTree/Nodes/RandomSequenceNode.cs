using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that evaluates all child nodes in random order 
	/// and returns Success if ALL child nodes return Success.
	/// </summary>
	public class RandomSequenceNode : ICompositeNode
	{
		private int currentChildIndex;
		private List<int> childrenLeftToRun;


		public RandomSequenceNode()
		{
			nodeType = NodeType.Sequence;
			childrenLeftToRun = new List<int>();
		}


		public override INode Init()
		{
			Debug.Log("RandomSequenceNode init");
			nodeState = NodeState.Running;

			childrenLeftToRun.Clear();
			for (int i = 0; i < children.Count; ++i)
			{
				childrenLeftToRun.Add(i);
			}

			currentChildIndex = Random.Range(0, childrenLeftToRun.Count);
			childrenLeftToRun.Remove(currentChildIndex);

			return children[currentChildIndex].Init();
		}


		/// <summary>
		/// If any child returns a failure, then this node returns a failure.
		/// </summary>
		/// <returns></returns>
		public override INode ChildFinished(NodeState childNodeState)
		{
			if (childNodeState == NodeState.Failure)
			{
				nodeState = NodeState.Failure;
				return this; // could just call Exit() here? while loop vs tail-end recursion?
			}

			if (childrenLeftToRun.Count == 0)
			{
				nodeState = NodeState.Failure;
				return this;
			}

			currentChildIndex = childrenLeftToRun[Random.Range(0, childrenLeftToRun.Count)];
			childrenLeftToRun.Remove(currentChildIndex);

			return children[currentChildIndex].Init();
		}

		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}