using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class WaveManager : MonoBehaviour
	{
		public GameObject enemyPrefab;

		private Wave blueSkullWave;
		private bool odd;

		void Start()
		{
			blueSkullWave = new Wave(enemyPrefab);

		}


		public void OnBeat()
		{
			blueSkullWave.OnBeat();
		}


		public void OnBar()
		{
			if (odd)
			{
				odd = false;
				LaunchWave();
			}
			else
				odd = true;
		}

		public void LaunchWave()
		{
			blueSkullWave.PrepareWave();
		}
	}
}