using UnityEngine;

namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A Composite Node that returns Success if any of it's children return Success.
	/// Evaluates all Nodes in order (or random, if selected) and returns on first child Success.
	/// </summary>
	public class SelectorNode : ICompositeNode
	{
		public bool random = false;
		private int currentChildIndex;


		public SelectorNode()
		{
			nodeType = NodeType.Selector;
		}


		public override INode Init()
		{
			Debug.Log("SelectorNode init");
			nodeState = NodeState.Running;
			currentChildIndex = 0;
			INode next = children[currentChildIndex].Init();
			if (next != null)
				return next;
			return null;
		}

		/// <summary>
		/// If any child reports a success, then this node returns a success.
		/// Otherwise, returns failure.
		/// </summary>
		/// <returns></returns>
		public override INode ChildFinished(NodeState childNodeState)
		{
			if (childNodeState == NodeState.Success)
			{
				nodeState = NodeState.Success;
				return this; // could just call Exit() here? while loop vs tail-end recursion?
			}

			// should ONLY be NodeState.Failure at this point or we have some bad implementation problems
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