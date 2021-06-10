using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	/// <summary>
	/// For simplicity, a NodeWindow should not store any data that is in a NodeEditorObject (ex: parent node, children, etc.)
	/// </summary>
	public abstract class NodeWindow
	{
		protected const float TITLEBAR_OFFSET = 15;
		public static Texture brokenBranchImage = EditorGUIUtility.FindTexture(
			OhBehaveEditorWindow.ImageFolder + "Node Broken Branch.png");
		public static Texture disconnectedBranchImage = EditorGUIUtility.FindTexture(
			OhBehaveEditorWindow.ImageFolder + "Node Disconnected Branch.png");
		public static float DoubleClickTime = .3f;

		/// <summary>
		/// This should contain all the data needed for the game.
		/// Everything else is editor visual representation stuff only.
		/// </summary>
		public NodeEditorObject nodeObject;

		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public NodeStyle nodeStyle;
		public IParentNodeWindow parent;

		protected string nodeName;
		protected GUIStyle currentStyle;
		protected Color bgColor;
		protected GUIStyle labelStyle;

		protected OhBehaveTreeBlueprint treeBlueprint;
		protected bool isDragged;
		protected bool isSelected;
		/// <summary>
		/// If a child node is constructed before the parent the connection is invalid, 
		/// so hold off on the construction until it's possible.
		/// </summary>
		protected bool refreshConnection;
		protected bool isValid;

		private bool isConnectedToRoot;
		private string errorMsg;
		private double timeClicked = double.MinValue;


		public NodeWindow(NodeEditorObject nodeObj)
		{
			var ohBehave = EditorWindow.GetWindow<OhBehaveEditorWindow>();
			treeBlueprint = ohBehave.treeBlueprint;
			nodeObject = nodeObj;
			nodeName = nodeObj.displayName;

			switch (nodeObj.nodeType)
			{
				case NodeType.Leaf:
					nodeStyle = OhBehaveEditorWindow.LeafNodeStyle;
					bgColor = NodeStyle.LeafColor;
					labelStyle = NodeStyle.LeafLabelStyle;
					break;
				case NodeType.Selector:
					nodeStyle = OhBehaveEditorWindow.SelectorNodeStyle;
					bgColor = NodeStyle.SelectorColor;
					labelStyle = NodeStyle.SelectorLabelStyle;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Sequence:
					nodeStyle = OhBehaveEditorWindow.SequenceNodeStyle;
					bgColor = NodeStyle.SequenceColor;
					labelStyle = NodeStyle.SequencerLabelStyle;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
				case NodeType.Inverter:
					nodeStyle = OhBehaveEditorWindow.InverterNodeStyle;
					bgColor = NodeStyle.InverterColor;
					labelStyle = NodeStyle.InverterLabelStyle;
					outPoint = new ConnectionPoint(this, ConnectionPointType.Out, ConnectionControls.OnClickOutPoint);
					break;
			}

			currentStyle = nodeStyle.defaultStyle;

			if (nodeObject.index != OhBehaveTreeBlueprint.ROOT_INDEX)
				inPoint = new ConnectionPoint(this, ConnectionPointType.In, ConnectionControls.OnClickInPoint);

			NodeEditorObject prntObj = nodeObject.Parent;
			if (prntObj != null)
			{
				parent = (IParentNodeWindow)prntObj.GetWindow();
				if (parent == null)
					refreshConnection = true;
			}
			else
			{
				bgColor = NodeStyle.RootColor;
			}
		}


		/// <summary>
		/// Keep list of children update-to-date. Used by Composite Nodes.
		/// </summary>
		public abstract void UpdateChildrenList();
		public abstract bool ProcessEvents(Event e);
		public abstract void OnGUI();

		public virtual void DrawConnectionWires()
		{
			return;
		}



		public void Deselect()
		{
			isSelected = false;
			labelStyle.normal.textColor = Color.black;
		}

		public Rect GetRect()
		{
			Rect rect = nodeObject.windowRect;
			rect.position -= nodeObject.offset;
			return rect;
		}

		public Rect GetRectNoOffset()
		{
			return nodeObject.windowRect;
		}

		public virtual string GetName()
		{
			return nodeObject.displayName;
		}

		public bool TryGetParentName(out string parentName)
		{
			if (parent == null)
			{
				parentName = "";
				return false;
			}

			parentName = parent.GetName();
			return true;
		}

		public List<int> GetChildren()
		{
			return nodeObject.GetChildren();
		}

		public void ParentRemoved()
		{
			if (parent == null)
				return;

			parent = null;
		}

		public void SetParentWindow(IParentNodeWindow newParent)
		{
			if (parent != null)
			{ // TODO: cleanup old connection
				throw new Exception("Must handle situation where child already has parent!");
			}

			parent = newParent;
		}

		public void BranchBroken(bool isFine, bool isConnected, string errorCode)
		{
			isValid = isFine;
			isConnectedToRoot = isConnected;
			errorMsg = errorCode;
		}

		protected void RefreshConnection()
		{
			NodeEditorObject prntObj = nodeObject.Parent;
			if (prntObj != null)
			{
				parent = (IParentNodeWindow)prntObj.GetWindow();
				if (parent != null)
				{
					refreshConnection = false;
				}
			}
		}

		protected void LeftClick(Event e)
		{
			if (TitleLabelRect().Contains(e.mousePosition))
			{
				if (EditorApplication.timeSinceStartup - timeClicked <= DoubleClickTime)
				{
					timeClicked = double.MinValue;
					isDragged = false;

					NodeEditPopup.Init(nodeObject);
					return;
				}


				timeClicked = EditorApplication.timeSinceStartup;
				isDragged = true;
				GUI.changed = true;
				isSelected = true;
				labelStyle.normal.textColor = Color.white;
				treeBlueprint.SelectNode(nodeObject);
				currentStyle = nodeStyle.selectedStyle;
				Selection.SetActiveObjectWithContext(treeBlueprint, null);
				e.Use();
			}
			else if (GetRect().Contains(e.mousePosition))
			{
				GUI.changed = true;
				isSelected = true;
				labelStyle.normal.textColor = Color.white;
				treeBlueprint.SelectNode(nodeObject);
				currentStyle = nodeStyle.selectedStyle;
				Selection.SetActiveObjectWithContext(treeBlueprint, null);
				//e.Use();
			}
			else
			{
				GUI.changed = true;

				if (treeBlueprint.GetSelectedNode() == nodeObject)
				{
					Deselect();
					treeBlueprint.SelectNode(null);
				}

				currentStyle = nodeStyle.defaultStyle;
			}
		}


		protected void RightClick(Event e)
		{
			if (GetRect().Contains(e.mousePosition))
			{
				CreateNodeContextMenu();
				e.Use();
			}
		}

		protected void CreateNodeContextMenu()
		{
			var genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Delete Node"), false,
				() => treeBlueprint.DeleteNode(nodeObject));

			genericMenu.ShowAsContext();
		}

		protected void CreateTitleBar()
		{
			if (isValid)
				GUILayout.Space(TITLEBAR_OFFSET);
			else
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(isConnectedToRoot ? brokenBranchImage : disconnectedBranchImage);
				GUILayout.Label(errorMsg, OhBehaveEditorWindow.warningTextStyle);

				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(
					new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
						nodeObject.nodeType)),
					labelStyle
				);

				//if (GUILayout.Button("Edit", NodeStyle.LeafLabelStyle))
				//{
				//	NodeEditPopup.Init(nodeObject);
				//}
			}
			GUILayout.EndHorizontal();
		}

		protected Rect TitleLabelRect()
		{
			Rect rect = GetRect();
			rect.height = EditorGUIUtility.singleLineHeight + TITLEBAR_OFFSET;
			return rect;
		}

		protected void Drag(Vector2 delta)
		{
			nodeObject.MoveWindowPosition(delta, treeBlueprint.childrenMoveWithParent);
		}
	}
}