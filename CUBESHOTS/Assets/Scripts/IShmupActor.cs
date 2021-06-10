using AtomosZ.Cubeshots.WeaponSystems;

namespace AtomosZ.Cubeshots
{
	public interface IShmupActor
	{
		void TakeDamage(int damage);
		void BulletBecameInactive(BasicBullet bullet);
	}
}