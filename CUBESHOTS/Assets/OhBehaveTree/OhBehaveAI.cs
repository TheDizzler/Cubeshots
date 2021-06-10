using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace AtomosZ.OhBehave
{
	[RequireComponent(typeof(OhBehaveActions))]
	public class OhBehaveAI : MonoBehaviour
	{
		/// <summary>
		/// Filepath relative to Streaming Assets folder.
		/// </summary>
		public string jsonFilepath;
		public ICompositeNode root;


		private INode currentNode;


		void Start()
		{
			if (!File.Exists(Application.streamingAssetsPath + jsonFilepath))
			{
				Debug.LogError("ERROR: OhBehaveFile does not exist: "
					+ Application.streamingAssetsPath + jsonFilepath);
				return;
			}

			StreamReader reader = new StreamReader(Application.streamingAssetsPath + jsonFilepath);
			string fileString = reader.ReadToEnd();
			reader.Close();

			JsonBehaviourTree tree = JsonUtility.FromJson<JsonBehaviourTree>(fileString);

			Dictionary<int, INode> nodeDict = new Dictionary<int, INode>();
			List<INode> nodes = new List<INode>();

			var behaviourSource = GetComponent<OhBehaveActions>();
			foreach (JsonNodeData nodeData in tree.tree)
			{
				INode newNode;
				switch (nodeData.nodeType)
				{
					case NodeType.Inverter:
						newNode = new InverterNode();
						break;
					case NodeType.Leaf:
						newNode = new LeafNode(behaviourSource);
						Type sourceType = tree.actionSource.GetType();
						if (sourceType.GetMethod(nodeData.methodInfoName) == null)
							throw new Exception("Could not find method by name " + nodeData.methodInfoName);
						((LeafNode)newNode).actionInfo = sourceType.GetMethod(nodeData.methodInfoName);
						break;
					case NodeType.Selector:
						if (nodeData.isRandom)
							newNode = new RandomSelectorNode();
						else
							newNode = new SelectorNode();
						break;
					case NodeType.Sequence:
						if (nodeData.isRandom)
							newNode = new RandomSequenceNode();
						else
							newNode = new SequenceNode();
						break;
					default:
						throw new System.Exception("Node has no type!");
				}

				nodeDict[nodeData.index] = newNode;
			}

			foreach (JsonNodeData nodeData in tree.tree)
			{
				if (nodeData.childrenIndices != null && nodeData.childrenIndices.Length > 0)
				{
					ICompositeNode node = (ICompositeNode)nodeDict[nodeData.index];
					node.children = new List<INode>();
					foreach (int child in nodeData.childrenIndices)
					{
						nodeDict[child].parent = node;
						node.children.Add(nodeDict[child]);
					}
				}

				if (nodeData.parentIndex == -69)
				{ // I am root!
					root = (ICompositeNode)nodeDict[nodeData.index];
					currentNode = root.Init();
				}
			}

		}



		public void Evaluate()
		{
			if (currentNode.Evaluate() != NodeState.Running)
			{
				INode nextNode = currentNode.Exit();

				int whileLoops = 0;
				while (nextNode.nodeState != NodeState.Running)
				{
					whileLoops++;
					if (whileLoops > 50)
						throw new System.Exception("Behavior tree stuck in loop - cannot find valid node to run");
					if (nextNode == root)
					{
						Debug.Log("Behaviour start from beginning");
						nextNode = nextNode.Init();
						break;
					}

					nextNode = nextNode.Exit();
				}

				currentNode = nextNode;
			}
		}



#if UNITY_EDITOR
		/// <summary>
		/// This refreshes every gui update. Could def make more efficient.
		/// </summary>
		/// <returns></returns>
		public string[] GetMethodNames()
		{
			List<string> sharedMethodNames = new List<string>();
			foreach (MethodInfo element in GetComponent<OhBehaveActions>().GetType().GetMethods(
				BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
			{
				foreach (var param in element.GetParameters())
				{
					if (param.ParameterType == typeof(LeafNode))
					{ // at least one of the params must be a LeafNode
						sharedMethodNames.Add(element.Name);
						break;
					}
				}
			}

			return sharedMethodNames.ToArray();
		}
#endif
	}
}