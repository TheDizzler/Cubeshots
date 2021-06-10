namespace AtomosZ.OhBehave
{
	/// <summary>
	/// A complete, valid behavior tree for an OhBehaveAI actor.
	/// </summary>
	[System.Serializable]
	public class JsonBehaviourTree
	{
		public string name;
		public string blueprintGUID;

		public OhBehaveActions actionSource;
		public JsonNodeData rootNode;
		public JsonNodeData[] tree;
	}

	[System.Serializable]
	public class JsonNodeData
	{
		public int index;
		public int parentIndex;
		public int[] childrenIndices;
		public bool isRandom;
		public NodeType nodeType;
		public string methodInfoName;
	}
}
