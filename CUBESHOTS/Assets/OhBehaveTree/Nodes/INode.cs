namespace AtomosZ.OhBehave
{
	/// <summary>
	/// Used for the construction.
	/// </summary>
	public enum NodeType
	{
		Selector, Sequence,
		Inverter, 
		/// <summary>
		/// Not yet implemented.
		/// </summary>
		Succeeder,
		/// <summary>
		/// Not yet implemented.
		/// </summary>
		Repeater,
		/// <summary>
		/// Not yet implemented.
		/// </summary>
		ConditionalRepeater,

		Leaf,
	}

	public enum NodeState { Failure, Success, Running }

	public abstract class INode
	{
		public IParentNode parent;
		/// <summary>
		/// A descriptive name that represents what this node hopes to accomplish.
		/// Probably not needed but could help with debugging.
		/// </summary>
		public string name;
		/// <summary>
		/// Used for the editor.
		/// </summary>
		public NodeType nodeType { get; protected set; }
		public NodeState nodeState;

		/// <summary>
		/// The way currently implemented, only one leaf node can be evaluated per Update,
		/// insteading immediately falling through to the next leaf if that one is invalid.
		/// </summary>
		/// <returns></returns>
		public abstract INode Init();
		public abstract NodeState Evaluate();
		public abstract INode Exit();
	}
}