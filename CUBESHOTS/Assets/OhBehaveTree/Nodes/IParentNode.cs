namespace AtomosZ.OhBehave
{
	public interface IParentNode
	{
		void AddNode(INode newNode);
		void RemoveNode(INode removeNode);
		INode ChildFinished(NodeState childNodeState);
	}
}