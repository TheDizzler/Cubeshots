using System.Collections.Generic;
using AtomosZ.Cubeshots.GUI;
using AtomosZ.Cubeshots.PlayerLibs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ.Cubeshots
{
	public class PlayerManager : MonoBehaviour
	{
		public static PlayerManager instance;

		public GameObject playerPrefab;
		public GameObject cubePrefab;
		public Color[] cubeColors;
		public Vector2 playerStartPosition;

		public List<InputDevice> assignedGamepads = new List<InputDevice>();
		private int players;

		//[HideInInspector]
		//public List<Player> players;



		void Start()
		{
			instance = this;

			foreach (Joystick joystick in Joystick.all)
			{
				Debug.Log("joystick found: " + joystick);
				assignedGamepads.Add(joystick);
			}

			// enable a player for each gamepad connected
			foreach (Gamepad gamepad in Gamepad.all)
			{
				Debug.Log("gamepad found: " + gamepad.displayName);
				assignedGamepads.Add(gamepad);
			}

			assignedGamepads.Add(Keyboard.current);


			var inputManager = GetComponent<PlayerInputManager>();
			inputManager.playerPrefab = playerPrefab;


			for (int i = 0; i < assignedGamepads.Count; ++i)
			{
				string joyOrPad = assignedGamepads[i] is Joystick ? "Joystick" : "Gamepad";
				PlayerInput newPlayer = inputManager.JoinPlayer(i, 0, joyOrPad, assignedGamepads[i]);
				// rest of setup gets done in OnPlayerJoined.
			}
		}


		public void OnPlayerJoined(PlayerInput newPlayer)
		{
			++players;
			Debug.Log("Hello player " + players + "!");

			newPlayer.gameObject.name = newPlayer.currentControlScheme;
			PlayerCube cube = Instantiate(
				cubePrefab, playerStartPosition, Quaternion.identity).GetComponent<PlayerCube>();
			cube.SetColor(cubeColors[players - 1]);
			newPlayer.GetComponent<PlayerController>().Possess(cube);
			cube.GetComponent<Health>().healthBar = 
				GameObject.FindGameObjectWithTag(Tags.PLAYER_HEALTH_BAR).GetComponent<HealthBar>();
			//players.Add(newPlayer.GetComponent<Player>());
			//newPlayer.GetComponent<Player>().AddControllableActors(controllableActors);
		}


		public void OnPlayerLeft(PlayerInput newPlayer)
		{
			Debug.Log("Player left: " + newPlayer.gameObject.name);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.DrawCube(playerStartPosition, new Vector3(.1f, .1f, .1f));
		}


	}
}