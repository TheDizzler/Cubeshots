using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class Enemy : MonoBehaviour, ShmupActor
	{
		private const int BASIC_BULLET_STORE_SIZE = 10;

		public GameObject bulletPrefab;

		//public GameObject bulletPrefab;
		[HideInInspector]
		public Wave wave;

		private Health health;
		private bool isQuarter;
		private bool isBar;
		private int bulletsFired;
		private float nextAngle;

		private float timeAlive;
		private Camera mainCamera;


		void Start()
		{
			mainCamera = Camera.main;
			gameObject.SetActive(false);

			health = GetComponent<Health>();
		}


		public void Launch(Vector3 startPosition, Vector3 initialVelocity)
		{
			transform.position = startPosition;
			health.Reset();

			bulletsFired = 0;
			nextAngle = 195f;
			bulletsFired = 0;
			timeAlive = 0;

			gameObject.SetActive(true);
			GetComponent<Rigidbody2D>().velocity = initialVelocity;
		}

		public void OnSongStart()
		{
			bulletsFired = 0;
			nextAngle = 195f;
			bulletsFired = 0;
		}

		public void OnQuarterBeat()
		{
			isQuarter = true;
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

		void Update()
		{
			if (isBar)
			{
				bulletsFired = 0;
				nextAngle = 195f;


			}
			GetComponent<Rigidbody2D>().velocity = new Vector3(5, 1, 0);
			if (isQuarter && bulletsFired < 3)
			{
				++bulletsFired;
				nextAngle -= 15f;
				BasicBullet bullet = wave.GetNextBullet();
				bullet.SetOwner(this);
				if (bullet != null)
					bullet.Fire(transform.position, Quaternion.Euler(0, 0, nextAngle) * Vector2.up, 0);
			}

			isQuarter = false;
			isBar = false;

			timeAlive += Time.deltaTime;

			Vector3 vpPos = mainCamera.WorldToViewportPoint(transform.localPosition);
			if (vpPos.x < -.5f || vpPos.y < -.5f
				|| vpPos.x > 1.5f || vpPos.y > 1.5f)
			{
				gameObject.SetActive(false);
				wave.EnemyBecameInactive(this);
			}
		}
	}
}