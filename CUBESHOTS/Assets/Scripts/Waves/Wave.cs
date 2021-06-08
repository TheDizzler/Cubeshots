using System.Collections.Generic;
using AtomosZ.Cubeshots.MusicCommander;
using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class Wave
	{
		private const int BULLET_STORE_SIZE = 100;
		public static Vector3 storePosition
			= new Vector3(float.MinValue, float.MinValue, 0);


		public int enemyPerWave = 2;
		//protected BasicBullet[] bulletStore;
		private Queue<BasicBullet> bulletStore;
		/// <summary>
		/// The enemy prefab this wave is composed of.
		/// </summary>
		private GameObject enemyPrefab;
		private Vector3 leftSideLaunch;
		private Vector3 rightSideLaunch;
		private Queue<Enemy> nextWave = new Queue<Enemy>();
		private Queue<Enemy> enemyStore = new Queue<Enemy>();



		public Wave(GameObject prefab)
		{
			enemyPrefab = prefab;
			leftSideLaunch = Camera.main.ViewportToWorldPoint(
				new Vector3(0, .5f, -Camera.main.transform.position.z));
			rightSideLaunch = Camera.main.ViewportToWorldPoint(
				new Vector3(1, .5f, -Camera.main.transform.position.z));

			bulletStore = new Queue<BasicBullet>();

			for (int i = 0; i < enemyPerWave; ++i)
			{
				Enemy enemy = GameObject.Instantiate(
					enemyPrefab, storePosition, Quaternion.identity).GetComponent<Enemy>();
				enemy.wave = this;
				//enemy.bulletStore = bulletStore;
				enemyStore.Enqueue(enemy);
			}

			var bulletPrefab = enemyStore.Peek().bulletPrefab;
			var musicCommander = MusicalCommander.instance;


			for (int i = 0; i < BULLET_STORE_SIZE; ++i)
			{
				GameObject bullet = GameObject.Instantiate(
					bulletPrefab, storePosition,
					Quaternion.identity, musicCommander.bulletStore);
				bullet.tag = Tags.ENEMY_BULLET;
				bulletStore.Enqueue(bullet.GetComponent<BasicBullet>());
			}
		}

		public BasicBullet GetNextBullet()
		{
			if (bulletStore.Count == 0)
			{
				Debug.LogError(enemyPrefab.name + " bullet store not big enough");
			}

			return bulletStore.Dequeue();
		}

		public void OnBeat()
		{
			if (nextWave.Count > 0)
			{
				nextWave.Dequeue().Launch(leftSideLaunch, new Vector3(10, 1, 0));
				nextWave.Dequeue().Launch(rightSideLaunch, new Vector3(-10, 1, 0));
			}
		}

		public void PrepareWave()
		{
			if (enemyStore.Count < enemyPerWave)
			{
				for (int i = 0; i < enemyPerWave; ++i)
				{
					Enemy enemy = GameObject.Instantiate(
						enemyPrefab, storePosition, Quaternion.identity).GetComponent<Enemy>();
					enemy.wave = this;
					enemyStore.Enqueue(enemy);
				}
			}

			//for (int i = 0; i < enemyPerWave; ++i)
			//{
			nextWave.Enqueue(enemyStore.Dequeue());
			nextWave.Enqueue(enemyStore.Dequeue());
			//}
		}

		public void EnemyBecameInactive(Enemy enemy)
		{
			enemyStore.Enqueue(enemy);
		}

		public void BulletBecameInactive(BasicBullet bullet)
		{
			bullet.transform.localPosition = storePosition;
			bulletStore.Enqueue(bullet);
		}
	}
}