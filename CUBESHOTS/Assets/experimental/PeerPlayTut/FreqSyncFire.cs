using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.ExperimentalStuff.AudioTools
{
	public class FreqSyncFire : MonoBehaviour
	{
		public int band;
		public float triggerValue;
		public float timeBetweenShots;
		public GameObject projectile;
		public SpriteRenderer strip;

		public float colorMultiplier = 10;


		private AudioPeer audioPeer;
		private SpriteRenderer spriteRenderer;
		private float timeSinceFired;


		void Start()
		{
			audioPeer = GameObject.FindGameObjectWithTag("AudioCommander").GetComponent<AudioPeer>();
			spriteRenderer = GetComponent<SpriteRenderer>();
		}


		void Update()
		{
			timeSinceFired += Time.deltaTime;
			spriteRenderer.color = Color.Lerp(Color.white, audioPeer.bandColor[band], audioPeer.audioBandBuffer8[band] * colorMultiplier);
			strip.color = Color.Lerp(Color.black, audioPeer.bandColor[band], audioPeer.audioBandBuffer8[band] * colorMultiplier);
			if (timeSinceFired > timeBetweenShots && audioPeer.freqBand8[band] > triggerValue)
			{
				BasicBullet spark = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<BasicBullet>();
				spark.GetComponent<SpriteRenderer>().color = audioPeer.bandColor[band];
				spark.direction = -transform.right;
				timeSinceFired = 0;

			}
		}
	}
}