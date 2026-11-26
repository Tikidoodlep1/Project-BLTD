using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class is a server-only class. The TowerController should never run on the client or listen to client packets.
/// </summary>
[RequireComponent(typeof(Tower))]
[DisallowMultipleComponent]
public class TowerController : NetworkBehaviour
{
	Tower tower;

	private ITowerTargetable prevTarget = null;
	ITowerTargetable PrevTarget
	{
		get 
		{ 
			return prevTarget;
		}

		set
		{
			prevTarget = value;
		}
	}

	private ITowerTargetable currTarget = null;
	ITowerTargetable CurrTarget 
	{
		get 
		{ 
			return this.currTarget; 
		}

		set 
		{
			this.PrevTarget = this.currTarget;
			this.currTarget = value;
		}
	}

	private long lastAttackTicks = 0L;

	private void Awake()
	{
		this.tower = GetComponent<Tower>();
	}

    void FixedUpdate()
    {
		if (IsSpawned && this.tower.IsSpawned) 
		{
			UpdateTowerAI();
			TurnTowerTowardsTarget();
		}
    }

	public virtual void UpdateTowerAI()
	{
		if (currTarget == null || !currTarget.IsCurrentlyValidTarget())
		{
			this.GetNewTarget();
		}
		else if (this.IsAttackReady())
		{
			this.tower.AttackTarget(currTarget, lastAttackTicks);
			this.lastAttackTicks = DateTime.Now.Ticks;
		}
	}

	public virtual void GetNewTarget()
	{
		IList<ITowerTargetable> targets = tower.GetTargetables();
		if (targets.Count > 0)
		{
			this.currTarget = tower.GetTargetables()[0];
		}
	}

	public virtual void TurnTowerTowardsTarget()
	{
		if (this.IsAttackReady())
		{
			//Send a packet to turn the tower
		}
	}

	public bool IsAttackReady()
	{
		return (float)(DateTime.Now.Ticks - this.lastAttackTicks) / (float)TimeSpan.TicksPerSecond <= this.tower.GetAttackInterval();
	}
}
