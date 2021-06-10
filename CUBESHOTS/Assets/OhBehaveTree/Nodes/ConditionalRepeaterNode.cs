namespace AtomosZ.OhBehave
{
	/// <summary>
	/// Like a repeater, these decorators will continue to reprocess their child.
	/// That is until the child finally returns a failure,
	/// at which point the repeater will return success to its parent.
	/// </summary>
	public class ConditionalRepeaterNode : IDecoratorNode
	{
		public ConditionalRepeaterNode()
		{
			nodeType = NodeType.ConditionalRepeater;
		}

		public override INode Init()
		{
			nodeState = NodeState.Running;
			return child.Init();
		}

		public override NodeState Evaluate()
		{
			return nodeState;
		}

		public override INode ChildFinished(NodeState childNodeState)
		{
			if (childNodeState == NodeState.Failure)
			{
				nodeState = NodeState.Success;
				return Exit();
			}

			return Init();
		}


		public override INode Exit()
		{
			throw new System.NotImplementedException();
		}


	}
}