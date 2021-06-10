using UnityEngine;

namespace AtomosZ.OhBehave
{
	public abstract class OhBehaveActions : MonoBehaviour
	{
		public OhBehaveAI bsm;


		void Update()
		{
			bsm.Evaluate();
		}
	}
}