using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.WeaponSystems
{
	public class FireBullet : MonoBehaviour
	{
		public GameObject bulletPrefab;
		public Vector3 bulletSpawnOffset;

		public float cooldown = .25f;

		[HideInInspector]
		public BasicBullet[] bulletStore;
		int bulletCount = 20;

		private int nextBulletIndex = 0;
		private float timeFired;


		void Start()
		{
			bulletStore = new BasicBullet[bulletCount];
			for (int i = 0; i < bulletCount; ++i)
			{
				bulletStore[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<BasicBullet>();
			}
		}

		public void Fire()
		{
			//if (Time.time - timeFired < cooldown)
			//	return;
			BasicBullet bullet = GetNextBullet();
			if (bullet == null)
				return;
			bullet.Fire(transform.position + bulletSpawnOffset, -transform.right, 0);
			timeFired = Time.time;
		}

		private BasicBullet GetNextBullet()
		{
			bool restarted = false;
			while (bulletStore[nextBulletIndex].enabled)
			{
				if (++nextBulletIndex >= bulletCount)
				{
					nextBulletIndex = 0;

					if (restarted)
					{
						Debug.Log(name + " needs bigger bullet store");
						return null;
						//return Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<BasicBullet>();
					}

					restarted = true;
				}
			}

			return bulletStore[nextBulletIndex];
		}
	}
}