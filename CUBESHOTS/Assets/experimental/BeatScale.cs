using System.Collections;
using UnityEngine;

namespace AtomosZ.Cubeshots.AudioTools
{
	public class BeatScale : MonoBehaviour
	{
		public Vector3 beatScale;
		public Vector3 restScale;

		/// <summary>
		/// How long for visualization to hit peak.
		/// </summary>
		public float attackTime;
		/// <summary>
		/// How long for visualization to return to rest.
		/// </summary>
		public float decayTime;

		private Coroutine moveToScale;
		private float toRestTime;
		private bool isBeat;


		public void OnBeat()
		{
			if (moveToScale != null)
				StopCoroutine(moveToScale);
			moveToScale = StartCoroutine(MoveToScale(beatScale));
		}

		void Update()
		{
			if (isBeat)
				return;

			toRestTime += Time.deltaTime;
			transform.localScale = Vector3.Lerp(transform.localScale, restScale, toRestTime / decayTime);
		}


		private IEnumerator MoveToScale(Vector3 target)
		{
			isBeat = true;

			Vector3 current = transform.localScale;
			Vector3 initial = current;
			float timer = 0;

			while (current != target)
			{
				current = Vector3.Lerp(initial, target, timer / attackTime);
				transform.localScale = current;
				timer += Time.deltaTime;

				yield return null;
			}

			toRestTime = 0;
			isBeat = false;
		}
	}
}