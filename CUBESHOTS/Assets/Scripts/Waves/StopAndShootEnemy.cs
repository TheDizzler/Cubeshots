using AtomosZ.Cubeshots.MusicCommander;
using AtomosZ.Cubeshots.WeaponSystems;
using AtomosZ.OhBehave;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class StopAndShootEnemy : Enemy
	{
		public float exitSpeed;

		private Vector3 startPos;
		private Vector3 endPos;
		private Vector3 exitPos;
		private float timeToDestination;
		private float timeStart;



		public void Launch(Vector3 startPosition, Vector3 destination)
		{
			Reset();
			gameObject.SetActive(true);
			startPos = startPosition;
			endPos = destination;
			exitPos = new Vector3(-endPos.x, 5, 0);
			transform.position = startPosition;

			timeToDestination = MusicalCommander.instance.fullNoteLength;
			timeStart = Time.time;
		}

		protected override void Reset()
		{
			base.Reset();
		}


		public void MoveToPosition(LeafNode node)
		{
			float t = timeAlive / timeToDestination;
			if (t >= 1 && isBar)
			{
				t = 1;
				node.nodeState = NodeState.Success;
			}

			transform.localPosition = Vector3.Lerp(startPos, endPos, t);
		}

		public void Fire(LeafNode node)
		{
			if (isBeat)
			{
				if (bulletsFired == 16)
				{
					node.nodeState = NodeState.Success;
					return;
				}

				BasicBullet bullet = wave.GetNextBullet();
				bullet.SetOwner(this);
				bullet.Fire(transform.position,
					(player.localPosition - transform.localPosition).normalized, 0);
				++bulletsFired;
			}
		}

		public void ExitStage(LeafNode node)
		{
			body.velocity =
				(exitPos - transform.localPosition).normalized * exitSpeed;

			CheckOutOfBounds();
		}
	}
}