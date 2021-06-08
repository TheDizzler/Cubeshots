using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AtomosZ.Cubeshots.Editors
{
	[InitializeOnLoad]
	public class SceneAutoLoader : MonoBehaviour
	{
		private static readonly string MasterScene = "Assets/Scenes/MasterScene.unity";

		// Static constructor binds a playmode-changed callback.
		// [InitializeOnLoad] above makes sure this gets executed.
		static SceneAutoLoader()
		{
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		private static void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
			{
				for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
				{
					Debug.Log(EditorSceneManager.GetSceneAt(i).path);
					if (EditorSceneManager.GetSceneAt(i).path == MasterScene)
					{
						return;
					}
				}
				Debug.Log("Adding MasterScene");
				EditorSceneManager.OpenScene(MasterScene, OpenSceneMode.Additive);
			}
		}
	}
}