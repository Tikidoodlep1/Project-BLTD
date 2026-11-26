using UnityEngine;

public interface ITowerTargetable
{
	public int GetHealth();
	public bool IsCurrentlyValidTarget();
	public Transform GetTowerTargetTracker();
	public void OnTowerAttacked(TowerEffects attack);
}
