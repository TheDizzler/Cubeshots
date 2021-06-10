using UnityEngine;

namespace AtomosZ.Cubeshots.WeaponSystems
{
	public class BasicBullet : MonoBehaviour
	{
		public float speed;
		public int damage = 10;
		public float timeToLive = 3;

		[HideInInspector]
		public IShmupActor owner;
		[HideInInspector]
		public Vector3 direction;
		private float timeAlive;
		private float curve = 0;
		private Camera mainCamera;
		private Transform player;


		void Start()
		{
			mainCamera = Camera.main;
			gameObject.SetActive(false);
			enabled = false;
			player = GameObject.FindGameObjectWithTag(Tags.PLAYER).transform;
		}

		public void SetOwner(IShmupActor actor)
		{
			owner = actor;
		}

		public void Fire(Vector3 position, Vector3 direction, float curveAmount)
		{
			transform.localPosition = position;
			this.direction = direction;
			curve = curveAmount;
			timeAlive = 0;
			enabled = true;
			gameObject.SetActive(true);
		}

		void Update()
		{
			transform.localPosition = transform.localPosition + direction * speed * Time.deltaTime;
			direction = Quaternion.Euler(0, 0, curve * Time.deltaTime) * direction;
			timeAlive += Time.deltaTime;
			Debug.DrawLine(transform.localPosition, player.localPosition);
			Vector3 vpPos = mainCamera.WorldToViewportPoint(transform.localPosition);
			if (vpPos.x < -.5f || vpPos.y < -.5f
				|| vpPos.x > 1.5f || vpPos.y > 1.5f)
				Disable();

			
		}

		void OnTriggerEnter2D(Collider2D collision)
		{
			if ((collision.CompareTag(Tags.PLAYER) && CompareTag(Tags.ENEMY_BULLET))
				|| (collision.CompareTag(Tags.ENEMY) && CompareTag(Tags.PLAYER_BULLET)))
			{
				collision.GetComponent<IShmupActor>().TakeDamage(damage);
				Disable();
			}
		}

		private void Disable()
		{
			gameObject.SetActive(false);
			enabled = false;
			owner.BulletBecameInactive(this);
		}
	}
}