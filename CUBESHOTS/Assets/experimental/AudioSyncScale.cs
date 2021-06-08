using System.Collections;
using UnityEngine;

namespace AtomosZ.Cubeshots.AudioTools
{
	public class AudioSyncScale : AudioSyncer
	{
		public Vector3 beatScale;
		public Vector3 restScale;

		private Coroutine moveToScale;
		private float toRestTime;

		private IEnumerator MoveToScale(Vector3 target)
		{
			Vector3 current = transform.localScale;
			Vector3 initial = current;
			float timer = 0;

			while (current != target)
			{
				current = Vector3.Lerp(initial, target, timer / timeToBeat);
				transform.localScale = current;
				timer += Time.deltaTime;

				yield return null;
			}

			toRestTime = 0;
			isBeat = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (isBeat)
				return;

			toRestTime += Time.deltaTime;
			transform.localScale = Vector3.Lerp(transform.localScale, restScale, toRestTime / restSmoothTime);
		}

		public override void OnBeat()
		{
			base.OnBeat();

			if (moveToScale != null)
				StopCoroutine(moveToScale);
			moveToScale = StartCoroutine(MoveToScale(beatScale));
		}
	}
}