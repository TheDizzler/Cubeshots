using UnityEngine;

namespace AtomosZ.Cubeshots.MusicCommander
{
	public class ScaleCubeNote : MonoBehaviour
	{
		public float frequency;
		public int noteIndex;

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
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumByNotesLeft[noteIndex]);
					transform.localScale = newScale;
					break;
				case 1:
					newScale = transform.localScale;
					newScale.y = Mathf.Lerp(1, maxYScale, musicalCommander.spectrumByNotesRight[noteIndex]);
					transform.localScale = newScale;
					break;
			}
		}
	}
}