using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.Cubeshots.GUI
{
	public class HealthBar : MonoBehaviour
	{
		public SpriteRenderer currentHealth;


		public void SetHealthPercent(float percent)
		{
			currentHealth.transform.localScale = new Vector3(percent, 1, 1);
		}
	}
}