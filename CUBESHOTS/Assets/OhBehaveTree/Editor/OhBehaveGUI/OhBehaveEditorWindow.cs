using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.OhBehave.EditorTools
{
	public class OhBehaveEditorWindow : EditorWindow
	{
		public const string UserNodeFolderKey = "UserNodeFolder";
		public static readonly string ImageFolder =
			"Assets/OhBehaveTree/Editor/OhBehaveGUI/Images/";

		private const float ZOOM_BORDER = 10;
		private const float AREA_BELOW_ZOOM_HEIGHT = 50;
		private const string DefaultNodeFolder = "OhBehaveTrees";

		public static NodeStyle SelectorNodeStyle;
		public static NodeStyle SequenceNodeStyle;
		public static NodeStyle LeafNodeStyle;
		public static NodeStyle InverterNodeStyle;
		public static GUIStyle InPointStyle;
		public static GUIStyle OutPointStyle;
		public static GUIStyle normalFoldoutStyle;
		public static GUIStyle invalidFoldoutStyle;
		public static GUIStyle warningTextStyle;

		private static string userNodeFolder;

		public OhBehaveTreeBlueprint treeBlueprint;
		public EditorZoomer zoomer;

		/// <summary>
		/// Delayed FileChooser open to avoid EditorGUILayout error.
		/// </summary>
		public bool openFileChooser;

		private OhBehaveAI chooseNewJsonFor;
		private OhBehaveEditorWindow window;
		private OhBehaveAI currentAIBehaviour;
		private OhBehaveAI aiRequestingNewBlueprint;
		private Rect zoomRect;
		private bool openSaveFileChooser;



		void OnEnable()
		{
			userNodeFolder = EditorPrefs.GetString(UserNodeFolderKey, "");
			if (userNodeFolder == "")
			{
				userNodeFolder = DefaultNodeFolder;
			}

			if (window != null)
			{ // no need to reconstruct everything
				return;
			}

			window = GetWindow<OhBehaveEditorWindow>();
			window.titleContent = new GUIContent("OhBehave!");

			try
			{
				if (EditorStyles.helpBox == null)
				{ //EditorStyle not yet initialized
					return;
				}
			}
			catch (System.Exception)
			{ //EditorStyle not yet initialized
				return;
			}

			CreateStyles();
			if (zoomer == null)
				zoomer = new EditorZoomer();
			if (treeBlueprint != null)
			{
				treeBlueprint.ConstructNodes();
				zoomer.Reset(treeBlueprint.zoomerSettings);
			}
		}

		private void CreateStyles()
		{
			SelectorNodeStyle = new NodeStyle();
			SelectorNodeStyle.Init(new Vector2(250, 100));
			SequenceNodeStyle = new NodeStyle();
			SequenceNodeStyle.Init(new Vector2(250, 100));
			LeafNodeStyle = new NodeStyle();
			LeafNodeStyle.Init(new Vector2(250, 75));
			InverterNodeStyle = new NodeStyle();
			InverterNodeStyle.Init(new Vector2(250, 75));

			InPointStyle = new GUIStyle();
			InPointStyle.normal.background =
				EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut normal.png");
			InPointStyle.hover.background =
				EditorGUIUtility.FindTexture(ImageFolder + "NodeInOut hover.png");

			OutPointStyle = new GUIStyle();
			OutPointStyle.normal.background = InPointStyle.normal.background;
			OutPointStyle.hover.background = InPointStyle.hover.background;

			normalFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			invalidFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			invalidFoldoutStyle.normal.textColor = Color.red;
			invalidFoldoutStyle.onNormal.textColor = Color.red;

			warningTextStyle = new GUIStyle();
			warningTextStyle.normal.textColor = Color.red;
		}


		public void OpenFileChooser(OhBehaveAI ohBehave)
		{
			openFileChooser = true;
			chooseNewJsonFor = ohBehave;
		}


		public bool Open(OhBehaveAI ohBehaveAI)
		{
			currentAIBehaviour = ohBehaveAI;

			treeBlueprint = GetBlueprintFor(ohBehaveAI);
			if (treeBlueprint == null)
			{
				Repaint();
				return false;
			}

			treeBlueprint.ohBehaveAI = ohBehaveAI;
			treeBlueprint.ConstructNodes();

			if (zoomer != null)
				zoomer.Reset(treeBlueprint.zoomerSettings);
			window.Show();
			Repaint();
			return true;
		}


		void Update()
		{
			if (Selection.activeGameObject != null)
			{
				OhBehaveAI ohBehaveSM = Selection.activeGameObject.GetComponent<OhBehaveAI>();
				if (ohBehaveSM != null && ohBehaveSM != currentAIBehaviour)
				{ // switch to the currently selected gameobjects behavior tree
					Open(ohBehaveSM);
					return;
				}
			}

			if (openFileChooser)
			{
				openFileChooser = false;
				string path = EditorUtility.OpenFilePanelWithFilters(
					"Choose new OhBehave file",
					"Assets/StreamingAssets/" + userNodeFolder,
					new string[] { "OhBehaveTree Json file", "OhJson" });

				if (!string.IsNullOrEmpty(path))
				{
					string relativePath = path.Replace(Application.streamingAssetsPath, "");
					chooseNewJsonFor.jsonFilepath = relativePath;
					Open(chooseNewJsonFor);
				}

				chooseNewJsonFor = null;
			}

			if (treeBlueprint == null)
			{
				if (Selection.activeObject != null)
				{
					OhBehaveTreeBlueprint testIfTree =
						Selection.activeObject as OhBehaveTreeBlueprint;
					if (testIfTree != null)
					{
						treeBlueprint = testIfTree;
						Repaint();
					}
				}
			}
			else
			{
				treeBlueprint.PendingDeletes();
				Repaint();
			}

			if (openSaveFileChooser)
			{
				if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + userNodeFolder))
				{
					if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/"))
						AssetDatabase.CreateFolder("Assets", "StreamingAssets");
					if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/" + DefaultNodeFolder))
						AssetDatabase.CreateFolder("Assets/StreamingAssets", DefaultNodeFolder);
					userNodeFolder = DefaultNodeFolder;
					EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
				}

				string nodename = "NewOhBehaveTree";
				int num = AssetDatabase.FindAssets(nodename,
					new string[] { "Assets/StreamingAssets/" + userNodeFolder }).Length;
				if (num != 0)
				{
					nodename += " (" + num + ")";
				}

				var path = EditorUtility.SaveFilePanelInProject(
					"Create New Json Behavior State Machine", nodename, "OhJson",
					"Where to save json file?", "Assets/StreamingAssets/" + userNodeFolder);
				if (path.Length != 0)
				{
					// check if user is using a folder that isn't the default
					if (Path.GetFileName(Path.GetDirectoryName(path)) != userNodeFolder)
					{
						userNodeFolder = Path.GetFileName(Path.GetDirectoryName(path));
						EditorPrefs.SetString(UserNodeFolderKey, userNodeFolder);
					}

					var machineBlueprint = CreateInstance<OhBehaveTreeBlueprint>();
					machineBlueprint.Initialize(aiRequestingNewBlueprint, path);

					Open(aiRequestingNewBlueprint);
				}

				aiRequestingNewBlueprint = null;
				openSaveFileChooser = false;
			}
		}


		void OnGUI()
		{
			if (InPointStyle == null)
			{
				CreateStyles();
			}

			{   // Just keeping this around for future reference.
				if (NodeEditPopup.instance != null)
				{
					if (Event.current.type == EventType.MouseDown
						&& EditorWindow.mouseOverWindow != NodeEditPopup.instance)
						NodeEditPopup.instance.Hide();
				}
			}

			if (treeBlueprint == null)
			{
				return;
			}

			if (zoomer == null)
			{
				zoomer = new EditorZoomer();
				zoomer.Reset(treeBlueprint.zoomerSettings);
			}

			zoomer.HandleEvents(Event.current);

			DrawHorizontalUILine(Color.gray);

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.Repaint)
			{
				zoomRect.position = new Vector2(
					ZOOM_BORDER,
					lastRect.yMax + lastRect.height + ZOOM_BORDER);
				zoomRect.size = new Vector2(
					window.position.width - ZOOM_BORDER * 2,
					window.position.height
						- (lastRect.yMax + ZOOM_BORDER * 2 + AREA_BELOW_ZOOM_HEIGHT));
			}


			zoomer.Begin(zoomRect);
			{
				treeBlueprint.OnGui(Event.current, zoomer);
			}
			zoomer.End(new Rect(
				0, zoomRect.yMax + zoomRect.position.y - 50,
				window.position.width, window.position.height));


			treeBlueprint.childrenMoveWithParent =
				EditorGUILayout.ToggleLeft("Reposition children with Parent",
					treeBlueprint.childrenMoveWithParent);
			EditorGUILayout.Vector2Field("mouse", Event.current.mousePosition);


			if (GUI.changed)
				Repaint();
		}


		public void OpenSaveFilePanel(OhBehaveAI instance)
		{
			openSaveFileChooser = true;
			aiRequestingNewBlueprint = instance;
		}


		void OnLostFocus()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (mouseOverWindow != null && mouseOverWindow.title == "Inspector")
#pragma warning restore CS0618 // Type or member is obsolete
				return;
			if (treeBlueprint != null)
				treeBlueprint.DeselectNode();
			Repaint();
		}



		public static void DrawHorizontalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.width -= 9.5f;
			r.y += padding / 2;
			//r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}

		public static void DrawVerticalUILine(Color color, int thickness = 2, int padding = 10)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
			r.width = thickness;
			//r.x += padding / 2;
			r.y -= 2;
			r.height += 6;
			EditorGUI.DrawRect(r, color);
		}


		/// <summary>
		/// Because it's not possible to store editor objects in non-editor objects
		/// this rigamoral is needed to find the BehaviorTree blueprint.
		/// </summary>
		/// <param name="ohBehaveAI"></param>
		/// <returns></returns>
		private OhBehaveTreeBlueprint GetBlueprintFor(OhBehaveAI ohBehaveAI)
		{
			if (string.IsNullOrEmpty(ohBehaveAI.jsonFilepath))
			{
				return null;
			}

			if (!File.Exists(Application.streamingAssetsPath + ohBehaveAI.jsonFilepath))
			{
				return null;
			}

			StreamReader reader = new StreamReader(
				Application.streamingAssetsPath + ohBehaveAI.jsonFilepath);
			string fileString = reader.ReadToEnd();
			reader.Close();

			JsonBehaviourTree tree = JsonUtility.FromJson<JsonBehaviourTree>(fileString);

			if (string.IsNullOrEmpty(tree.blueprintGUID))
			{
				Debug.LogError("No blueprints GUID");
				return null;
			}

			var blueprint = AssetDatabase.LoadAssetAtPath<OhBehaveTreeBlueprint>(
					AssetDatabase.GUIDToAssetPath(tree.blueprintGUID));

			return blueprint;
		}
	}
}