using UnityEngine;
using static AtomosZ.Cubeshots.MusicCommander.MusicalCommander;

namespace AtomosZ.Cubeshots.MusicCommander
{
	public class ScaleCube : MonoBehaviour
	{
		public SpectrumBand band;
		[Range(0, 1)]
		public int channel;
		public float maxYScale;

		private MusicalCommander musicalCommander;
		private Vector3 newScale;


		void Start()
		{
			musicalCommander = MusicalCommander.instance;
		}

		void Update()
		{
			switch (channel)
			{
				case 0:
					newScale = transform.localScale;
					//newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumBandsLeft[(int)band]);
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumHighs[(int)band]);
					transform.localScale = newScale;
					break;
				case 1:
					newScale = transform.localScale;
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumBandsRight[(int)band]);
					transform.localScale = newScale;
					break;
			}
		}
	}
}