using AtomosZ.Cubeshots.MusicCommander;
using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class BulletHellPattern : MonoBehaviour
	{
		public enum ReboundType
		{
			None,
			Timer,
			Angle,
			ManualTrigger,
			Beat,
		}


		public GameObject bulletPrefab;
		[Range(.1f, 1f)]
		public float fireDelay = .5f;
		[Range(1, 20)]
		public int bulletsPerVolley = 1;
		[Range(1, 359)]
		public float volleySpread = 5f;
		[Range(0, 100)]
		public float spinRate = 1f;
		[Range(0, 50)]
		public float spinAcceleration = 0f;
		[Range(0, 100)]
		public float maxSpinRate = 0f;
		[Range(0, 10)]
		public float reboundTime = 0f;
		[Range(5, 179f)]
		public float reboundAngle = 30f;
		[Range(-359, 359)]
		public float bulletCurve = 0f;
		public ReboundType reboundType = ReboundType.None;

		/// <summary>
		/// Is each bullet in array spread evenly over volleySpread degrees
		/// or are they seperated by volleySpread degrees?
		/// </summary>
		public bool isEvenAngleSpread;

		public int bulletStoreSize = 200;

		protected float currentSpinRate;
		protected Vector3 startDirectionAngle;
		protected Vector3 nextDirectionAngle = Vector3.right;
		protected bool reboundTriggered;
		protected float timeToRebound;

		private BasicBullet[] bulletStore;
		private int nextBulletIndex = 0;
		private float fireCount = 0;



		void Start()
		{
			Init();
		}

		public virtual void Init()
		{
			bulletStore = new BasicBullet[bulletStoreSize];
			for (int i = 0; i < bulletStoreSize; ++i)
			{
				bulletStore[i] = Instantiate(
					bulletPrefab, transform.position, Quaternion.identity, MusicalCommander.instance.bulletStore)
						.GetComponent<BasicBullet>();
			}
		}


		public virtual void TriggerRebound()
		{
			if (reboundType == ReboundType.ManualTrigger)
				reboundTriggered = true;
		}


		public virtual void Update()
		{
			fireCount -= Time.deltaTime;
			if (fireCount <= 0)
			{
				Fire();
			}

			nextDirectionAngle = Quaternion.AngleAxis(currentSpinRate * Time.deltaTime, Vector3.forward) * nextDirectionAngle;
			if (spinRate != 0)
			{
				currentSpinRate += spinAcceleration * Time.deltaTime;
				if (maxSpinRate != 0 && Mathf.Abs(currentSpinRate) > maxSpinRate)
				{
					currentSpinRate = maxSpinRate;
				}

				switch (reboundType)
				{
					case ReboundType.None:
						break;

					case ReboundType.Timer:
						timeToRebound -= Time.deltaTime;

						if (timeToRebound <= 0)
						{
							timeToRebound = reboundTime;
							currentSpinRate = -currentSpinRate;
						}
						break;

					case ReboundType.Angle:
						if (Vector3.Angle(nextDirectionAngle, startDirectionAngle) > reboundAngle)
						{
							currentSpinRate = -currentSpinRate;
							startDirectionAngle = nextDirectionAngle;
						}
						break;

					case ReboundType.ManualTrigger:
						if (reboundTriggered)
						{
							reboundTriggered = false;
							currentSpinRate = -currentSpinRate;
						}
						break;
				}
			}
		}


		public void Fire()
		{
			fireCount = fireDelay;
			float spreadInterval = volleySpread / (bulletsPerVolley - 1);
			Vector3 spreadDirection = nextDirectionAngle;
			for (int i = 0; i < bulletsPerVolley; ++i)
			{
				BasicBullet bullet = GetNextBullet();
				if (bullet == null)
					return;

				bullet.Fire(transform.position, spreadDirection, bulletCurve);
				if (isEvenAngleSpread)
					spreadDirection = Quaternion.AngleAxis(spreadInterval, Vector3.forward) * spreadDirection;
				else
					spreadDirection = Quaternion.AngleAxis(volleySpread, Vector3.forward) * spreadDirection;
			}
		}


		private BasicBullet GetNextBullet()
		{
			bool restarted = false;
			while (bulletStore[nextBulletIndex].enabled)
			{
				if (++nextBulletIndex >= bulletStoreSize)
				{
					nextBulletIndex = 0;

					if (restarted)
					{
						Debug.LogWarning(name + " needs a larger bullet store!!");
						return null;
					}

					restarted = true;
				}
			}

			return bulletStore[nextBulletIndex];
		}
	}
}