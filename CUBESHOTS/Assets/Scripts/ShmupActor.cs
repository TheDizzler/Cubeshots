using AtomosZ.Cubeshots.WeaponSystems;

namespace AtomosZ.Cubeshots
{
	public interface ShmupActor
	{
		void TakeDamage(int damage);
		void BulletBecameInactive(BasicBullet bullet);
	}
}