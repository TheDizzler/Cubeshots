using AtomosZ.Cubeshots.GUI;
using UnityEngine;

namespace AtomosZ.Cubeshots
{
	[RequireComponent(typeof(IShmupActor))]
	public class Health : MonoBehaviour
	{
		public int maxHealth = 100;
		public HealthBar healthBar;

		private int currentHealth;


		public void Reset()
		{
			currentHealth = maxHealth;
			healthBar.SetHealthPercent(currentHealth / maxHealth);
		}

		/// <summary>
		/// Return true if dead.
		/// </summary>
		/// <param name="damage"></param>
		/// <returns></returns>
		public bool TakeDamage(int damage)
		{
			if (currentHealth == 0)
			{
				Debug.Log("Already died once");
				return false;
			}

			currentHealth -= damage;
			if (currentHealth <= 0)
			{
				currentHealth = 0;
				Debug.Log(name + " died");
				healthBar.SetHealthPercent(0);
				return true;
			}

			healthBar.SetHealthPercent((float)currentHealth / maxHealth);
			return false;
		}
	}
}