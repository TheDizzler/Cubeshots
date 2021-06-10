namespace AtomosZ.OhBehave
{
	/// <summary>
	/// Always returns Success irrespective of child success.
	/// These are useful in cases where you want to process a branch of a
	/// tree where a failure is expected or anticipated, but you don’t
	/// want to abandon processing of a sequence that branch sits on.
	/// The opposite of this type of node is not required, as an inverter
	/// will turn a succeeder into a ‘failer’ if a failure is required for the parent.
	/// </summary>
	public class SucceederNode : IDecoratorNode
	{
		public SucceederNode()
		{
			nodeType = NodeType.Succeeder;
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
			nodeState = NodeState.Success;
			return Exit();
		}

		public override INode Exit()
		{
			return parent.ChildFinished(NodeState.Success);
		}



	}
}