using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.Cubeshots.PlayerLibs
{
	/// <summary>
	/// Get input from a player for an actor.
	/// </summary>
	public class PlayerController : MonoBehaviour
	{
		public PlayerCube actor;
		public InputActionReference moveAction;
		public InputActionReference fireAction;


		public void Possess(PlayerCube cube)
		{
			actor = cube;
			cube.controller = this;
		}


		public void UpdateCommands()
		{
			if (fireAction.action.ReadValue<float>() == 1)
				actor.fireDown = true;
		}

		public void FixedUpdateCommands()
		{
			actor.inputVector = moveAction.action.ReadValue<Vector2>();
		}


		public void OnQuit()
		{
			Application.Quit();
		}

		public void OnDeviceLost()
		{
			Debug.Log("controller lost!");
		}

		public void OnDeviceRegained()
		{
			Debug.Log("Controller rediscovered!");
		}

		public void OnControlsChanged()
		{
			Debug.Log("controls changed :0");
		}

	}
}