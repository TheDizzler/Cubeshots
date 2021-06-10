using UnityEngine;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that evaluates all child nodes and returns Success
	/// if ALL child nodes return Success.
	/// </summary>
	public class SequenceNode : ICompositeNode
	{
		private int currentChildIndex;

		public SequenceNode()
		{
			nodeType = NodeType.Sequence;
		}


		public override INode Init()
		{
			Debug.Log("SequenceNode init");
			nodeState = NodeState.Running;
			currentChildIndex = 0;
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

			if (++currentChildIndex >= children.Count)
			{
				nodeState = NodeState.Failure;
				return this; // could just call Exit() here? while loop vs tail-end recursion?
			}

			return children[currentChildIndex].Init();
		}

		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}