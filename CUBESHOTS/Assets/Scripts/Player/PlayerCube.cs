using System.Collections.Generic;
using AtomosZ.Cubeshots.MusicCommander;
using AtomosZ.Cubeshots.Waves;
using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.PlayerLibs
{
	public class PlayerCube : MonoBehaviour, ShmupActor
	{
		private const int BASIC_BULLET_STORE_SIZE = 100;

		public float speed = 10f;
		public GameObject bulletPrefab;


		[HideInInspector]
		public Vector2 inputVector;
		[HideInInspector]
		public bool fireDown;
		[HideInInspector]
		public PlayerController controller;

		private Queue<BasicBullet> bulletStore;
		//private int nextBulletIndex;
		private bool isEighthBeat;
		private bool isQuarterBeat;
		private bool isHalfBeat;
		private bool isFullBeat;
		private Rigidbody2D body;
		private MusicalCommander musicCommander;
		private Health health;


		void Start()
		{
			body = GetComponent<Rigidbody2D>();
			musicCommander = MusicalCommander.instance;
			health = GetComponent<Health>();
			health.Reset();

			bulletStore = new Queue<BasicBullet>();
			for (int i = 0; i < BASIC_BULLET_STORE_SIZE; ++i)
			{
				GameObject bulletGO = Instantiate(
					bulletPrefab, transform.position,
					Quaternion.identity, musicCommander.bulletStore);
				bulletGO.tag = Tags.PLAYER_BULLET;
				BasicBullet bullet = bulletGO.GetComponent<BasicBullet>();
				bullet.SetOwner(this);
				bulletStore.Enqueue(bullet);
			}
		}



		public void SetColor(Color color)
		{
			GetComponent<SpriteRenderer>().color = color;
		}


		public void TakeDamage(int damage)
		{
			if (health.TakeDamage(damage))
			{
				gameObject.SetActive(false);
			}
		}

		public void BulletBecameInactive(BasicBullet bullet)
		{
			bullet.transform.localPosition = Wave.storePosition;
			bulletStore.Enqueue(bullet);
		}

		public void IsEighthBeat()
		{
			isEighthBeat = true;
		}

		public void IsQuarterBeat()
		{
			isQuarterBeat = true;
		}

		public void IsHalfBeat()
		{
			isHalfBeat = true;
		}

		public void IsFullBeat()
		{
			isFullBeat = true;
		}


		void Update()
		{
			controller.UpdateCommands();

			if (fireDown)
			{
				if (isEighthBeat)
				{
					BasicBullet bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Vector2.up, 0);
				}

				if (isQuarterBeat)
				{
					BasicBullet bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, 15) * Vector2.up, 0);
					bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, -15) * Vector2.up, 0);
				}

				if (isHalfBeat)
				{
					BasicBullet bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, 45) * Vector2.up, 0);
					bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, -45) * Vector2.up, 0);
				}

				if (isFullBeat)
				{
					BasicBullet bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, 90) * Vector2.up, 0);
					bullet = GetNextBullet();
					if (bullet != null)
						bullet.Fire(transform.position, Quaternion.Euler(0, 0, -90) * Vector2.up, 0);
				}

				fireDown = false;
			}

			isEighthBeat = false;
			isQuarterBeat = false;
			isHalfBeat = false;
			isFullBeat = false;
		}


		void FixedUpdate()
		{
			controller.FixedUpdateCommands();

			body.velocity = inputVector * Time.deltaTime * speed;
			inputVector = Vector2.zero;
		}

		private void OnTriggerEnter(Collider other)
		{
			Debug.Log("Trigger");
		}



		private BasicBullet GetNextBullet()
		{
			if (bulletStore.Count == 0)
				Debug.LogError(name + " needs a larger bullet store!!");

			return bulletStore.Dequeue();
			//bool restarted = false;
			//while (bulletStore[nextBulletIndex].enabled)
			//{
			//	if (++nextBulletIndex >= BASIC_BULLET_STORE_SIZE)
			//	{
			//		nextBulletIndex = 0;

			//		if (restarted)
			//		{
			//			Debug.LogWarning(name + " needs a larger bullet store!!");
			//			return null;
			//		}

			//		restarted = true;
			//	}
			//}

			//return bulletStore[nextBulletIndex];
		}
	}
}