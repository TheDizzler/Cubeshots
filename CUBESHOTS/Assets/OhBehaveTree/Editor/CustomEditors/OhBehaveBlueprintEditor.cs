using System;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools.CustomEditors
{
	[CustomEditor(typeof(OhBehaveTreeBlueprint))]
	public class OhBehaveBlueprintEditor : Editor
	{
		private OhBehaveTreeBlueprint instance;
		private SerializedProperty nodeList;
		private SerializedProperty treeName;
		private SerializedProperty treeDesc;
		private SerializedProperty behaviourSource;



		void OnEnable()
		{
			instance = (OhBehaveTreeBlueprint)target;
			nodeList = serializedObject.FindProperty("savedNodes");
			treeName = serializedObject.FindProperty("behaviorTreeName");
			treeDesc = serializedObject.FindProperty("description");
			behaviourSource = serializedObject.FindProperty("behaviourSource");
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (instance.IsNodeSelected())
			{
				DrawSelectedNode();
			}
			else
			{
				DrawBlueprintEditor();
			}

			serializedObject.ApplyModifiedProperties();
		}


		private void DrawBlueprintEditor()
		{
			//base.OnInspectorGUI();
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", target, typeof(OhBehaveAI), false);
			EditorGUILayout.PropertyField(nodeList, true);
			GUI.enabled = true;

			EditorGUILayout.DelayedTextField(treeName);
			EditorGUILayout.DelayedTextField(treeDesc);


			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviourSource"));
			var newSource = (OhBehaveActions)serializedObject.FindProperty("behaviourSource").objectReferenceValue;
			instance.behaviourSource = newSource;

			if (EditorGUI.EndChangeCheck())
			{
				instance.EditorNeedsRefresh();
			}

			if (instance.sharedMethods != null && instance.sharedMethods.Count > 0)
			{
				// Create the dropdown in the inspector for the found methods
				EditorGUILayout.Popup("Function List", 0, instance.privateMethodNames.ToArray());
			}


			if (GUILayout.Button("Open In Editor"))
			{
				EditorWindow.GetWindow<OhBehaveEditorWindow>().Open(instance.ohBehaveAI);
			}
		}


		private void DrawSelectedNode()
		{
			var nodeObject = instance.GetSelectedNode();

			GUILayout.BeginVertical();
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label(
						new GUIContent(nodeObject.displayName + " - " + Enum.GetName(typeof(NodeType),
							nodeObject.nodeType)),
						NodeStyle.LeafLabelStyle
					);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					GUI.enabled = false;
					EditorGUILayout.IntField("Reference index", nodeObject.index);
					EditorGUILayout.IntField("Parent ref index", nodeObject.parentIndex);
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				{
					EditorGUIUtility.labelWidth = 40f;
					string newName = EditorGUILayout.DelayedTextField("Display Name", nodeObject.displayName);
					if (newName != nodeObject.displayName)
					{
						nodeObject.displayName = newName;
						nodeObject.RefreshParent();
					}
				}
				GUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Description:");
				string newDesc = EditorGUILayout.DelayedTextField(nodeObject.description);
				if (newDesc != nodeObject.description)
					nodeObject.description = newDesc;


				NodeType newType = (NodeType)EditorGUILayout.EnumPopup(nodeObject.nodeType);
				if (newType != nodeObject.nodeType)
				{
					nodeObject.ChangeNodeType(newType);
				}

				EditorGUIUtility.labelWidth = 0;

				var selectedNodeProperty = serializedObject.FindProperty("selectedNode");
				switch (nodeObject.nodeType)
				{
					case NodeType.Leaf:
						GUI.enabled = false;
						EditorGUILayout.ObjectField("Behaviour Source", instance.behaviourSource, typeof(OhBehaveActions), true);
						int methodIndex;

						if (string.IsNullOrEmpty(nodeObject.actionName))
						{
							EditorGUILayout.TextField("No method selected");
							methodIndex = 0;
						}
						else
						{
							EditorGUILayout.TextField(nodeObject.actionName);
							methodIndex = instance.sharedMethodNames.IndexOf(nodeObject.actionName);
						}

						GUI.enabled = true;

						if (instance.sharedMethods != null && instance.sharedMethods.Count > 0)
						{
							// Create the dropdown in the inspector for the found methods
							int newSelection = EditorGUILayout.Popup("Function List", methodIndex, instance.sharedMethodNames.ToArray());
							if (newSelection != methodIndex)
							{
								if (newSelection == 0)
									nodeObject.actionName = "";
								else
									nodeObject.actionName = instance.sharedMethodNames[newSelection];
							}
						}

						break;
					case NodeType.Selector:
					case NodeType.Sequence:
						GUI.enabled = false;
						EditorGUILayout.PropertyField(selectedNodeProperty.FindPropertyRelative("children"), true);
						GUI.enabled = true;
						break;
				}
			}
			GUILayout.EndVertical();
		}
	}
}