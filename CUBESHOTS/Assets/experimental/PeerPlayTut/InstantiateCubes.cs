using UnityEngine;

namespace AtomosZ.ExperimentalStuff.AudioTools
{
	public class InstantiateCubes : MonoBehaviour
	{
		public GameObject cubePrefab;
		public float maxScale;
		public bool useBuffer;
		private AudioPeer audioPeer;
		private int spectrumBands;
		private GameObject[] large;
		private GameObject[] small;
		private GameObject[] colors;



		public void Init(int bands)
		{
			audioPeer = GameObject.FindGameObjectWithTag("AudioCommander").GetComponent<AudioPeer>();
			spectrumBands = bands;
			

			var hertzPerBand = AudioSettings.outputSampleRate * .5f / AudioAnalyzer.spectrumSampleSize;
			Debug.Log("AudioSettings.outputSampleRate: " + AudioSettings.outputSampleRate
				+ " AudioAnalyzer.spectrumSampleSize: " + AudioAnalyzer.spectrumSampleSize
				+ hertzPerBand + "hz/band");


			small = new GameObject[audioPeer.freqBand8.Length];
			colors = new GameObject[audioPeer.freqBand8.Length];
			for (int i = 0; i < small.Length; ++i)
			{
				GameObject newCube = Instantiate(cubePrefab, transform);
				newCube.transform.position = new Vector3(i * 2 - 8, 1.5f, 0);
				newCube.name = "Freq " + i;
				small[i] = newCube;

				newCube = Instantiate(cubePrefab, transform);
				newCube.transform.position = new Vector3(i * 2 - 7f, -1.5f, 0);
				newCube.name = "Color " + i;
				colors[i] = newCube;
			}

			//large = new GameObject[spectrumBands];
			//float space = 360f / spectrumBands;
			//for (int i = 0; i < spectrumBands; ++i)
			//{
			//	GameObject newCube = Instantiate(cubePrefab, transform);

			//	newCube.name = "Band " + i + ": " + hertzPerBand * i + "Hz";
			//	large[i] = newCube;

			//	transform.eulerAngles = new Vector3(0, space * i, 0);
			//	newCube.transform.position = Vector3.forward * 50;
			//}



			this.enabled = true;
		}

		void Update()
		{
			//for (int i = 0; i < large.Length; ++i)
			//{
			//	large[i].transform.localScale = new Vector3(1, (AudioPeer.spectrum[i] * maxScale) + 1, 1);
			//}

			float[] values = useBuffer ? audioPeer.bandBuffer8 : audioPeer.freqBand8;
			for (int i = 0; i < small.Length; ++i)
			{
				small[i].transform.localScale = new Vector3(1, (values[i]) + 1, 1);

				float bufferValue = audioPeer.audioBandBuffer8[i];
				colors[i].transform.localScale = new Vector3(1, (bufferValue * maxScale) + 1, 1);
				Color color = new Color(bufferValue, bufferValue, bufferValue);
				var mat = colors[i].GetComponent<MeshRenderer>().materials[0];
				mat.SetColor("_EmissionColor", color);
			}


		}
	}
}