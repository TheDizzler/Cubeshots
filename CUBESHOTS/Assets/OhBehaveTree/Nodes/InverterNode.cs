namespace AtomosZ.OhBehave
{
	public class InverterNode : IDecoratorNode
	{
		public InverterNode()
		{
			nodeType = NodeType.Inverter;
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

			nodeState = NodeState.Failure;
			return Exit();
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}