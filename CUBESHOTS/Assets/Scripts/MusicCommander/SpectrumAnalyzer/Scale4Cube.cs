using UnityEngine;
using static AtomosZ.Cubeshots.MusicCommander.MusicalCommander;

namespace AtomosZ.Cubeshots.MusicCommander
{
	public class Scale4Cube : MonoBehaviour
	{
		public SpectrumBand4 band;
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
					//newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumBands4Left[(int)band]);
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumHighs4[(int)band]);
					transform.localScale = newScale;
					break;
				case 1:
					newScale = transform.localScale;
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumBands4Right[(int)band]);
					transform.localScale = newScale;
					break;
			}
		}
	}
}