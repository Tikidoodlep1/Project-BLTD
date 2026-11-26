using UnityEngine;
using Unity.Netcode;

[DisallowMultipleComponent]
public class TowerTargetableBase : NetworkBehaviour, ITowerTargetable
{
	//[SerializeField]
	public int health;
	public bool isTargetable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public virtual void OnTowerAttacked(TowerEffects attack)
	{
		//TODO
		Debug.Log("Tower Attack Target " + this.name + " - " + this.gameObject.name);
	}

	public virtual Transform GetTowerTargetTracker()
	{
		return this.gameObject.transform;
	}

	public virtual int GetHealth()
	{
		return this.health;
	}

	public virtual bool IsCurrentlyValidTarget()
	{
		return this.health > 0 && !isTargetable;
	}
}
