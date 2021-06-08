using TMPro;
using UnityEngine;

public class PeakFinder : MonoBehaviour
{
	public int targetFrequency;
	public TextMeshPro text;
	public AnimationCurve beatCurve;

	private float hertzPerBin;
	private int targetIndex;
	private float timeSinceLastBeat;


	void Start()
	{
		hertzPerBin = AudioSettings.outputSampleRate * .5f / AudioAnalyzer.spectrumSampleSize;
		targetIndex = (int)(targetFrequency / hertzPerBin);
		text.text = targetFrequency + "Hz";
	}

	public void FindPeak(SpectralFluxInfo spectralFluxInfo, int fluxFreqIndex)
	{
		if (fluxFreqIndex == -1)
		{
			if (spectralFluxInfo.isPeak)
			{
				timeSinceLastBeat = 0;
			}
		}
		else if (spectralFluxInfo.fluxes[fluxFreqIndex].isPeak)
		{
			timeSinceLastBeat = 0;
		}
	}


	void Update()
	{
		float eval = Mathf.Max(1, 1 + beatCurve.Evaluate(timeSinceLastBeat));
		transform.localScale = new Vector3(eval, eval, 1);

		timeSinceLastBeat += Time.deltaTime;
	}
}
