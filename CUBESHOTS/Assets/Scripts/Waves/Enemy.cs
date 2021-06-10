using AtomosZ.Cubeshots.WeaponSystems;
using AtomosZ.OhBehave;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public abstract class Enemy : OhBehaveActions, IShmupActor
	{
		private const int BASIC_BULLET_STORE_SIZE = 10;
		private const float OUT_OF_BOUNDS_TIME_LIMIT = 1.5f;

		public GameObject bulletPrefab;

		[HideInInspector]
		public Wave wave;

		[SerializeField]
		protected Health health = null;
		[SerializeField]
		protected Rigidbody2D body = null;
		protected Camera mainCamera;
		protected Transform player;
		protected float oobTimer;
		protected bool isBeat;
		protected bool isBar;
		protected float timeAlive;

		protected int bulletsFired;
		private float nextAngle;



		void Start()
		{
			mainCamera = Camera.main;
			gameObject.SetActive(false);
			player = GameObject.FindGameObjectWithTag(Tags.PLAYER).transform;
		}



		protected virtual void Reset()
		{
			health.Reset();
			isBeat = false;
			isBar = false;
			bulletsFired = 0;
			timeAlive = 0;
		}


		//public void Launch(Vector3 startPosition, Vector3 initialVelocity)
		//{
		//	transform.position = startPosition;
		//	health.Reset();

		//	bulletsFired = 0;
		//	nextAngle = 195f;
		//	bulletsFired = 0;
		//	timeAlive = 0;

		//	isQuarter = false;
		//	isBar = false;

		//	gameObject.SetActive(true);
		//	body.velocity = initialVelocity;
		//}

		/// <summary>
		/// For debugging.
		/// </summary>
		public void OnSongStart()
		{
			bulletsFired = 0;
			nextAngle = 195f;
			bulletsFired = 0;
		}

		public void OnQuarterBeat()
		{
			isBeat = true;
		}

		public void OnBarBeat()
		{
			isBar = true;
		}

		public void TakeDamage(int damage)
		{
			if (health.TakeDamage(damage))
			{
				gameObject.SetActive(false);
				wave.EnemyBecameInactive(this);
			}
		}

		public void BulletBecameInactive(BasicBullet bullet)
		{
			wave.BulletBecameInactive(bullet);
		}

		void LateUpdate()
		{
			timeAlive += Time.deltaTime;
			isBeat = false;
			isBar = false;
		}



		protected void CheckOutOfBounds()
		{
			// Check if actor is out of camera
			Vector3 vpPos = mainCamera.WorldToViewportPoint(transform.localPosition);
			if (vpPos.x < -.05f || vpPos.y < -.05f
				|| vpPos.x > 1.05f || vpPos.y > 1.05f)
			{
				oobTimer += Time.deltaTime;
				if (oobTimer >= OUT_OF_BOUNDS_TIME_LIMIT)
				{
					gameObject.SetActive(false);
					wave.EnemyBecameInactive(this);
				}
			}
			else
				oobTimer = 0;
		}
	}
}