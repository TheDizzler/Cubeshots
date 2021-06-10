using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static AtomosZ.OhBehave.EditorTools.NodeEditorObject;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// The visual representation of our Behavior Tree. This is an editor only object.
	/// This allows for "loose" nodes. Only the non-"loose" nodes get commited to
	/// the game BehaviorStateMachine.
	/// </summary>
	public class OhBehaveTreeBlueprint : ScriptableObject
	{
		public const int ROOT_INDEX = 0;
		public const int MISSING_INDEX = -2;
		public const int NO_PARENT_INDEX = -1;
		/// <summary>
		/// Used to help find when reached root.
		/// </summary>
		public const int ROOT_NODE_PARENT_INDEX = -69;
		public const int DefaultTreeRowHeight = 75;
		public const string blueprintsPrefix = "BTO_";

		public static string blueprintsPath = "Assets/OhBehaveTree/Editor/_Blueprints";


		[Tooltip("Descriptive name. Has no in-game impact.")]
		public string behaviorTreeName;
		[Tooltip("Descriptive description. Has no impact in game.")]
		public string description;

		public OhBehaveActions behaviourSource;
		public List<MethodInfo> sharedMethods = null;
		public List<MethodInfo> privateMethods = null;
		public List<string> sharedMethodNames = null;
		public List<string> privateMethodNames = null;

		/// <summary>
		/// The current OhBehaveAI observing this blueprint.
		/// Note: multiple OhBehaveAIs may use the same blueprint.
		/// </summary>
		public OhBehaveAI ohBehaveAI;
		/// <summary>
		/// Blueprint and OhJson are 1:1.
		/// Not currently used.
		/// </summary>
		public string jsonGUID;
		public List<NodeEditorObject> savedNodes;
		public ZoomerSettings zoomerSettings;
		public bool childrenMoveWithParent = true;
		public bool save;

		[SerializeField]
		private NodeEditorObject selectedNode;
		[SerializeField]
		private JsonBehaviourTree jsonTreeData;


		private BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

		private Dictionary<int, NodeEditorObject> nodeObjects;
		private SerializedObject serializedObject;
		/// <summary>
		/// For keeping track of indices when assigning an index to a new node.
		/// </summary>
		[SerializeField]
		private int lastNodeIndex = ROOT_INDEX;
		private List<NodeEditorObject> deleteTasks = new List<NodeEditorObject>();
		private ConnectionPoint startConnection;
		private ConnectionPoint endConnection;
		private Vector2 savedMousePos;

		private bool saveJsonData;


		public void ConstructNodes()
		{
			if (nodeObjects == null)
			{
				InitializeNodeDictionary();
			}

			serializedObject = new SerializedObject(this);

			if (nodeObjects.Count == 0)
			{
				if (OhBehaveEditorWindow.SequenceNodeStyle == null)
					return;

				var winData = new Rect(
							-OhBehaveEditorWindow.SequenceNodeStyle.size.x / 2,
							-OhBehaveEditorWindow.SequenceNodeStyle.size.y,
							OhBehaveEditorWindow.SequenceNodeStyle.size.x,
							OhBehaveEditorWindow.SequenceNodeStyle.size.y);

				NodeEditorObject newNode = new NodeEditorObject(NodeType.Sequence, ROOT_INDEX)
				{
					description = "The Root Node - where it all begins",
					displayName = "Root",
					windowRect = winData,
				};

				AddNewNode(newNode, 0);
				jsonTreeData.rootNode = new JsonNodeData { nodeType = NodeType.Sequence };

				zoomerSettings = new ZoomerSettings();
				AssetDatabase.Refresh();
				EditorUtility.SetDirty(this);
				save = true;
			}
		}


		public void EditorNeedsRefresh()
		{
			sharedMethods = null;
			sharedMethodNames = null;
			GetFunctions();
		}

		public void GetFunctions()
		{
			if (behaviourSource == null)
			{
				if (ohBehaveAI != null)
				{
					behaviourSource = ohBehaveAI.GetComponent<OhBehaveActions>();
				}
				else
				{
					sharedMethods = null;
					privateMethods = null;
					sharedMethodNames = null;
					privateMethodNames = null;
					return;
				}
			}


			sharedMethods = new List<MethodInfo>();
			privateMethods = new List<MethodInfo>();
			sharedMethodNames = new List<string>();
			privateMethodNames = new List<string>();

			foreach (MethodInfo element in behaviourSource.GetType().GetMethods(flags))
			{
				foreach (var param in element.GetParameters())
				{
					if (param.ParameterType == typeof(LeafNode))
					{ // at least one of the params must be a LeafNode
						privateMethods.Add(element);
						privateMethodNames.Add(element.Name);
						break;
					}
				}
			}

			sharedMethods.Add(null);
			sharedMethods.AddRange(privateMethods);
			sharedMethodNames.Add("No action selected");
			sharedMethodNames.AddRange(privateMethodNames);
		}

		private void OnEnable()
		{
			if (sharedMethods == null)
				GetFunctions();
		}


		public void OnGui(Event current, EditorZoomer zoomer)
		{
			if (serializedObject == null)
				serializedObject = new SerializedObject(this);

			if (nodeObjects == null)
			{
				ConstructNodes();
				return;
			}

			bool isValidTree = true;
			List<InvalidNodeMessage> errorMsgs = new List<InvalidNodeMessage>();
			// draw connections between nodes
			foreach (var node in nodeObjects.Values)
			{
				node.Offset(zoomer.GetContentOffset());
				if (node.ProcessEvents(current))
					save = true;
				if (!node.CheckIsValid(out InvalidNodeMessage invalidMsg))
				{
					isValidTree = false;
					errorMsgs.Add(invalidMsg);
				}

				node.DrawConnectionWires();
			}

			// draw rest
			foreach (var node in nodeObjects.Values)
			{
				node.OnGUI();
			}


			if (startConnection != null)
			{// we want to draw the line on-top of everything else
				Handles.DrawAAPolyLine(ConnectionControls.lineThickness,
					startConnection.rect.center,
					current.mousePosition);


				GUI.changed = true;
				if (current.button == 1
					&& current.type == EventType.MouseDown)
				{
					startConnection.isCreatingNewConnection = false;
					startConnection = null;
					endConnection = null;
				}
				else if (endConnection != null)
				{
					CompleteConnection();
				}
				else if (current.button == 0
					&& current.type == EventType.MouseUp)
				{
					// if this has not been consumed we can (?) assume that
					//	the mouse was not released over a connection point
					savedMousePos = current.mousePosition;
					startConnection.isCreatingNewConnection = false;

					if (startConnection.type == ConnectionPointType.Out)
						CreateChildContextMenu(startConnection.nodeWindow.nodeObject, true);
					else
						CreateParentContextMenu(startConnection.nodeWindow.nodeObject, true);
					startConnection = null;
				}
			}
			else if (current.button == 1
				&& current.type == EventType.MouseUp
				&& !zoomer.isScreenMoved)
			{
				savedMousePos = current.mousePosition;
				CreateStandAloneContextMenu();
			}

			zoomer.DisplayInvalid(isValidTree, errorMsgs);
			zoomer.Update(zoomerSettings);

			if (save)
			{
				Save(isValidTree);
				save = false;
			}
		}


		public void SelectNode(NodeEditorObject nodeObject)
		{
			if (IsNodeSelected() && selectedNode != nodeObject)
			{
				selectedNode.GetWindow().Deselect();
			}

			selectedNode = nodeObject;
		}

		public void DeselectNode()
		{
			if (!IsNodeSelected())
				return;
			selectedNode.GetWindow().Deselect();
			selectedNode = null;
		}



		public void StartPointSelected(ConnectionPoint selectedConnection)
		{
			if (startConnection != null)
			{
				Debug.LogError("Trying to make new start point");
				return;
			}

			startConnection = selectedConnection;
		}

		public void EndPointSelected(ConnectionPoint endPoint)
		{
			if (startConnection == null)
			{// probably mouse up over node after mouse down elsewhere				
				return;
			}

			if (endPoint.type != startConnection.type
				&& endPoint.nodeWindow != startConnection.nodeWindow)
			{
				endConnection = endPoint;
			}
			else
			{ // cancel this new connection
				startConnection.isCreatingNewConnection = false;
				startConnection = null;
			}
		}


		public bool IsValidConnection(ConnectionPoint hoveredPoint)
		{
			if (startConnection == null || hoveredPoint == startConnection)
				return true; // no points have been selected or this is the first selected point
			return (hoveredPoint.type != startConnection.type
				&& hoveredPoint.nodeWindow != startConnection.nodeWindow);
		}

		private void CompleteConnection()
		{

			NodeEditorObject nodeParent, nodeChild;
			if (startConnection.type == ConnectionPointType.Out)
			{
				nodeParent = startConnection.nodeWindow.nodeObject;
				nodeChild = endConnection.nodeWindow.nodeObject;
			}
			else
			{
				nodeParent = endConnection.nodeWindow.nodeObject;
				nodeChild = startConnection.nodeWindow.nodeObject;
			}

			// check for loop
			NodeEditorObject check = nodeParent;
			while (check != null && check.index != ROOT_INDEX)
			{
				check = check.Parent;
				if (check == nodeChild)
				{
					Debug.LogWarning("No loop for you!");
					startConnection.isCreatingNewConnection = false;
					endConnection = null;
					startConnection = null;
					return;
				}
			}

			if (nodeParent.nodeType == NodeType.Inverter && nodeParent.HasChildren())
			{
				// orphan the olde child
				var oldChild = GetNodeObjectByIndex(nodeParent.GetChildren()[0]);
				NodeEditorObject.DisconnectNodes(nodeParent, oldChild);
			}

			var oldParent = GetNodeObjectByIndex(nodeChild.parentIndex);
			if (oldParent != null)
			{
				// remove from old parent
				NodeEditorObject.DisconnectNodes(oldParent, nodeChild);
			}

			// ok, let's do this
			NodeEditorObject.ConnectNodes(nodeParent, nodeChild);

			startConnection.isCreatingNewConnection = false;
			endConnection = null;
			startConnection = null;
			save = true;
		}


		private void CreateStandAloneContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false,
				() => CreateStandAloneNode(NodeType.Leaf));
			genericMenu.AddItem(new GUIContent("Add Inverter"), false,
				() => CreateStandAloneNode(NodeType.Inverter));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false,
				() => CreateStandAloneNode(NodeType.Sequence));
			genericMenu.AddItem(new GUIContent("Add Selector"), false,
				() => CreateStandAloneNode(NodeType.Selector));
			genericMenu.ShowAsContext();
		}

		public void CreateParentContextMenu(NodeEditorObject childNode, bool createAtMousePosition = false)
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Inverter"), false,
				() => CreateParentNode(childNode, NodeType.Inverter, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false,
				() => CreateParentNode(childNode, NodeType.Sequence, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Selector"), false,
				() => CreateParentNode(childNode, NodeType.Selector, createAtMousePosition));
			genericMenu.ShowAsContext();
		}

		public void CreateChildContextMenu(NodeEditorObject parentNode, bool createAtMousePosition = false)
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Leaf"), false,
				() => CreateChildNode(parentNode, NodeType.Leaf, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Inverter"), false,
				() => CreateChildNode(parentNode, NodeType.Inverter, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Sequence"), false,
				() => CreateChildNode(parentNode, NodeType.Sequence, createAtMousePosition));
			genericMenu.AddItem(new GUIContent("Add Selector"), false,
				() => CreateChildNode(parentNode, NodeType.Selector, createAtMousePosition));
			genericMenu.ShowAsContext();
		}

		public NodeEditorObject GetNodeObjectByIndex(int nodeIndex)
		{
			if (nodeIndex <= OhBehaveTreeBlueprint.NO_PARENT_INDEX)
			{
				return null;
			}

			if (!nodeObjects.TryGetValue(nodeIndex, out NodeEditorObject node))
			{
				Debug.LogError("GetNodeObject() Error! " + nodeIndex + " does not exist in!");
				return null;
			}

			return node;
		}


		public void DeleteNode(NodeEditorObject node)
		{
			deleteTasks.Add(node);
		}

		private void Save(bool isValidTree)
		{
			//if (isValidTree)
			{
				List<JsonNodeData> tree = new List<JsonNodeData>();
				jsonTreeData.actionSource = behaviourSource;
				jsonTreeData.rootNode = AddNodeToTreeWithChildren(
					GetNodeObjectByIndex(ROOT_INDEX), ROOT_NODE_PARENT_INDEX, ref tree);

				jsonTreeData.tree = tree.ToArray();
				StreamWriter writer = new StreamWriter(Application.streamingAssetsPath + ohBehaveAI.jsonFilepath);
				writer.WriteLine(JsonUtility.ToJson(jsonTreeData, true));
				writer.Close();
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}

		private JsonNodeData AddNodeToTreeWithChildren(NodeEditorObject node, int parentIndex, ref List<JsonNodeData> tree)
		{
			JsonNodeData nodeData = new JsonNodeData
			{
				index = node.index,
				nodeType = node.nodeType,
				isRandom = node.isRandom,
				methodInfoName = node.actionName,
			};

			nodeData.parentIndex = parentIndex;

			tree.Add(nodeData);

			if (!node.HasChildren())
				return nodeData;

			List<JsonNodeData> childrenData = new List<JsonNodeData>();
			foreach (var nodeIndex in node.GetChildren())
			{
				NodeEditorObject childNode = GetNodeObjectByIndex(nodeIndex);
				childrenData.Add(AddNodeToTreeWithChildren(childNode, nodeData.index, ref tree));
			}

			nodeData.childrenIndices = node.GetChildren().ToArray();
			return nodeData;
		}

		public void PendingDeletes()
		{
			if (deleteTasks.Count == 0)
				return;
			foreach (var node in deleteTasks)
			{
				if (node.parentIndex == ROOT_NODE_PARENT_INDEX)
				{
					Debug.LogWarning("Delete denied: I am Root");
					continue;
				}

				node.NotifyFamilyOfDelete();


				if (!nodeObjects.Remove(node.index))
				{
					Debug.LogWarning(node.displayName + " index " + node.index + " was NOT found.");
				}

				savedNodes.Remove(node);
			}

			GUI.changed = true;
			deleteTasks.Clear();
			save = true;
		}

		private void CreateStandAloneNode(NodeType nodeType)
		{
			switch (nodeType)
			{
				case NodeType.Leaf:
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset(),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};

					AddNewNode(newNode, lastNodeIndex);

					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}
		}

		private void CreateParentNode(NodeEditorObject childNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect childWindowRect = childNode.GetWindow().GetRectNoOffset();
			NodeEditorObject.DisconnectNodes(childNode.Parent, childNode);
			save = true;
			switch (nodeType)
			{
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(createAtMousePosition ?
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset() :
								new Vector2(childWindowRect.x,
									childWindowRect.y - childWindowRect.height - DefaultTreeRowHeight),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};
					AddNewNode(newNode, lastNodeIndex);

					NodeEditorObject.ConnectNodes(newNode, childNode);
					break;

				case NodeType.Leaf:
					throw new Exception("A leaf may not be a parent");
				default:
					Debug.LogWarning("TODO: CreateParentNode of type " + nodeType);
					break;
			}
		}

		private void CreateChildNode(NodeEditorObject parentNode, NodeType nodeType, bool createAtMousePosition)
		{
			Rect parentWindowRect = parentNode.GetWindow().GetRectNoOffset();
			if (parentNode.nodeType == NodeType.Inverter)
			{
				if (parentNode.HasChildren())
				{
					DisconnectNodes(parentNode, GetNodeObjectByIndex(parentNode.GetChildren()[0]));
				}
			}

			switch (nodeType)
			{
				case NodeType.Leaf:
				case NodeType.Sequence:
				case NodeType.Selector:
				case NodeType.Inverter:
					NodeEditorObject newNode
						= new NodeEditorObject(nodeType, ++lastNodeIndex)
						{
							description = nodeType + " type node. Add description of desired behaviour",
							displayName = nodeType.ToString(),
							windowRect = new Rect(createAtMousePosition ?
								savedMousePos + EditorWindow.GetWindow<OhBehaveEditorWindow>().zoomer.GetContentOffset() :
								new Vector2(parentWindowRect.x,
									parentWindowRect.y + parentWindowRect.height + DefaultTreeRowHeight),
								OhBehaveEditorWindow.SequenceNodeStyle.size)
						};

					AddNewNode(newNode, lastNodeIndex);
					NodeEditorObject.ConnectNodes(parentNode, newNode);
					break;

				default:
					Debug.LogWarning("TODO: CreateChildNode of type " + nodeType);
					break;
			}
		}


		public bool IsNodeSelected()
		{
			return selectedNode != null && !string.IsNullOrEmpty(selectedNode.displayName);
		}

		public NodeEditorObject GetSelectedNode()
		{
			return selectedNode;
		}


		private void InitializeNodeDictionary()
		{
			nodeObjects = new Dictionary<int, NodeEditorObject>();
			foreach (var node in savedNodes)
			{
				nodeObjects[node.index] = node;
				if (node.index > lastNodeIndex)
					lastNodeIndex = node.index;
			}
		}

		private void AddNewNode(NodeEditorObject newNode, int nodeIndex)
		{
			nodeObjects.Add(nodeIndex, newNode);
			savedNodes.Add(newNode);
			save = true;
		}


		/// <summary>
		/// Only called when first constructed.
		/// </summary>
		/// <param name="behaviourAI"></param>
		/// <param name="newJsonFilepath"></param>
		public void Initialize(OhBehaveAI behaviourAI, string jsonFilepath)
		{
			if (!AssetDatabase.IsValidFolder(blueprintsPath))
			{
				string guid = AssetDatabase.CreateFolder(
					Path.GetDirectoryName(blueprintsPath),
					Path.GetFileName(blueprintsPath));
				blueprintsPath = AssetDatabase.GUIDToAssetPath(guid);
			}


			AssetDatabase.CreateAsset(this,
				blueprintsPath + "/" + blueprintsPrefix
					+ Path.GetFileNameWithoutExtension(jsonFilepath)
					+ GetInstanceID() + ".asset");


			string blueprintGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this));


			ohBehaveAI = behaviourAI;
			behaviourSource = ohBehaveAI.GetComponent<OhBehaveActions>();


			savedNodes = new List<NodeEditorObject>();


			jsonTreeData = new JsonBehaviourTree();
			jsonTreeData.name = Path.GetFileNameWithoutExtension(jsonFilepath);
			jsonTreeData.blueprintGUID = blueprintGUID;
			string jsonString = JsonUtility.ToJson(jsonTreeData, true);

			StreamWriter writer = new StreamWriter(jsonFilepath);
			writer.WriteLine(jsonString);
			writer.Close();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			string relativePath = jsonFilepath.Replace("Assets/StreamingAssets", "");
			ohBehaveAI.jsonFilepath = relativePath;
			jsonGUID = AssetDatabase.AssetPathToGUID(jsonFilepath);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(this);
		}
	}
}