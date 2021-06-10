namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A repeater will reprocess its child node each time its child returns a result.
	/// These are often used at the very base of the tree, to make the tree to run
	/// continuously. Repeaters may optionally run their children a set number of
	/// times before returning to their parent.
	/// </summary>
	public class RepeaterNode : IDecoratorNode
	{
		private bool isInfiniteRepeat;
		private int numRepeat;
		private int repeatCount;


		public RepeaterNode(bool isInfinite, int numRepeats)
		{
			nodeType = NodeType.Repeater;
			isInfiniteRepeat = isInfinite;
			numRepeat = numRepeats;
		}

		public override INode Init()
		{
			repeatCount = 0;
			nodeState = NodeState.Running;
			return child.Init();
		}

		public override NodeState Evaluate()
		{
			return nodeState;
		}

		public override INode ChildFinished(NodeState childNodeState)
		{
			if (isInfiniteRepeat || ++repeatCount >= numRepeat)
			{
				return child.Init();
			}

			nodeState = NodeState.Success; // should always return success?
			return Exit();
		}

		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}


	}
}