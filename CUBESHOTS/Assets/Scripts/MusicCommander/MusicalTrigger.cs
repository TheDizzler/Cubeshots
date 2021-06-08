using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.Cubeshots.MusicCommander.MusicalCommander;

namespace AtomosZ.Cubeshots.MusicCommander
{
	public class MusicalTrigger : MonoBehaviour
	{
		public List<TriggerEvent> musicalEvents;


		void Start()
		{
			foreach (var musicEvent in musicalEvents)
				MusicalCommander.instance.RegisterForEvents(musicEvent);
		}
	}
}