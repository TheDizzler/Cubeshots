using System.Collections.Generic;
using AtomosZ.Cubeshots.MusicCommander;
using AtomosZ.Cubeshots.WeaponSystems;
using UnityEngine;

namespace AtomosZ.Cubeshots.Waves
{
	public class Wave
	{
		public static Vector3 storePosition
			= new Vector3(float.MinValue, float.MinValue, 0);

		private const int BULLET_STORE_SIZE = 100;

		public int enemyPerWave = 4;
		private Queue<BasicBullet> bulletStore;
		/// <summary>
		/// The enemy prefab this wave is composed of.
		/// </summary>
		private GameObject enemyPrefab;
		private Vector3 leftSideLaunch;
		private Vector3 rightSideLaunch;
		private Queue<StopAndShootEnemy> nextWave = new Queue<StopAndShootEnemy>();
		private Queue<StopAndShootEnemy> enemyStore = new Queue<StopAndShootEnemy>();
		private int insufficientBulletCount;



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
				CreateEnemy();
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
				++insufficientBulletCount;
				Debug.LogError(enemyPrefab.name + " bullet store not big enough." +
					" Increase by " + insufficientBulletCount);
				GameObject bullet = GameObject.Instantiate(
					enemyStore.Peek().bulletPrefab, storePosition,
					Quaternion.identity, MusicalCommander.instance.bulletStore);
				bullet.tag = Tags.ENEMY_BULLET;
				bulletStore.Enqueue(bullet.GetComponent<BasicBullet>());
			}

			return bulletStore.Dequeue();
		}


		private List<Vector3> destinations = new List<Vector3>()
		{
			new Vector3(6, 1, 0),
			new Vector3(2, 1.5f, 0),
			new Vector3(-2, 1.5f, 0),
			new Vector3(-6, 1, 0)
		};
		private int nextDestination;

		public void OnBeat()
		{
			if (nextWave.Count > 0)
			{
				Vector3 launchPos = leftSideLaunch;
				nextWave.Dequeue().Launch(launchPos, destinations[nextDestination++]);
			}
			else
				nextDestination = 0;
		}

		public void PrepareWave()
		{
			if (enemyStore.Count < enemyPerWave)
			{
				for (int i = 0; i < enemyPerWave; ++i)
				{
					CreateEnemy();
				}
			}

			for (int i = 0; i < enemyPerWave; ++i)
				nextWave.Enqueue(enemyStore.Dequeue());
		}


		public void EnemyBecameInactive(Enemy enemy)
		{
			enemyStore.Enqueue((StopAndShootEnemy)enemy);
		}

		public void BulletBecameInactive(BasicBullet bullet)
		{
			bullet.transform.localPosition = storePosition;
			bulletStore.Enqueue(bullet);
		}


		private void CreateEnemy()
		{
			StopAndShootEnemy enemy = GameObject.Instantiate(
				enemyPrefab, storePosition, Quaternion.identity)
				.GetComponent<StopAndShootEnemy>();
			enemy.wave = this;
			enemyStore.Enqueue(enemy);
		}
	}
}