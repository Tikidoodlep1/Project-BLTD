using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// <para>The class in charge of telling the tower what it is hitting as the result of an attack.</para>
/// 
/// <para>Due to the need to serialize this class, TowerAttackEffector classes must remain stateless, other than keeping their registry name or 
/// serialize their own stateful data.</para>
/// 
/// <para>To aid in the statelessness of a TowerAttackEffector, the target, the tower, and the attack progress are passed to 
/// <see cref="StartAttack(ITowerTargetable, Tower, float)"/> and <seealso cref="UpdateAttack(ITowerTargetable, Tower, float)"/></para>
/// </summary>
public abstract class TowerAttackEffector : INetworkSerializable, IRegistryType
{
	public const string INSTANT = "INSTANT";
	public const string CIRCLE_WAVE_GROW = "CIRCLE_WAVE_GROW";
	public const string CIRCLE_WAVE_SHRINK = "CIRCLE_WAVE_SHRINK";
	public const string SWIPE = "SWIPE";
	public const string SWIPE_RETURN = "SWIPE_RETURN";

	public string name;

	public TowerAttackEffector(string name)
	{
		name = name.Replace(" ", "_");
		name = name.ToUpper();

		this.name = name;
	}

	public string GetRegistryName()
	{
		return this.name;
	}

	public virtual void NetworkSerialize<T>(BufferSerializer<T> buf) where T : IReaderWriter
	{
		buf.SerializeValue(ref name);
	}

	/// <summary>
	/// Runs when the attack target is chosen and the attack is performed. For attacks that DO NOT need to shapecast multiple times, use this.
	/// </summary>
	/// <param name="target">The primary target of the attack</param>
	/// <param name="attackPct">The attack completion as a percentage [0.0, 1.0].</param>
	public virtual ITowerTargetable[] StartAttack(ITowerTargetable target, Tower tower, float attackPct)
	{ return null; }
	/// <summary>
	/// Runs when the attack target is chosen and the attack is performed. For attacks that DO need to shapecast multiple times, use this.
	/// </summary>
	/// <param name="target">The primary target of the attack</param>
	/// <param name="attackPct">The attack completion as a percentage [0.0, 1.0].</param>
	public virtual ITowerTargetable[] UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{ return null; }
}

class InstantTowerAttackEffector : TowerAttackEffector
{
	public InstantTowerAttackEffector() : base(INSTANT) { }

	public override ITowerTargetable[] StartAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		int targetCount = tower.GetTowerAttackSettings().GetTowerAttackType().GetMaxTargets(target, tower);
		if (targetCount == 0)
		{
			return new ITowerTargetable[0];
		}

		//CircleCast, BoxCast, or CapsuleCast based on the tower attack size, then exclude targets within the tower attack distance
		Transform t = target.GetTowerTargetTracker();
		//By default, this method returns the results sorted by distance in ascending order (IE. the closest hit is first in the array)
		RaycastHit2D[] outer = Physics2D.CircleCastAll(
			new Vector2(t.position.x, t.position.y), 
			tower.GetAttackSize() + tower.GetAttackSizeDistance(), 
			Vector2.zero);
		List<ITowerTargetable> selectedTargets = new List<ITowerTargetable>();

		//Always prioritize the original target
		foreach (RaycastHit2D hit in outer)
		{
			if (hit.collider.gameObject.GetEntityId() == target.GetTowerTargetTracker().gameObject.GetEntityId())
			{
				//If the original target is not a valid hit, do not include it and break
				if (hit.distance >= tower.GetAttackSizeDistance())
				{
					selectedTargets.Add(target);
				}
				else
				{
					break;
				}
			}
		}

		//Pick targets by distance (closest first)
		foreach (RaycastHit2D hit in outer)
		{
			ITowerTargetable hitTarget = hit.collider.gameObject.GetComponent<ITowerTargetable>();
			if (hitTarget == null)
			{
				continue;
			}

			if (hit.distance >= tower.GetAttackSizeDistance())
			{
				selectedTargets.Add(hitTarget);
			}

			if (selectedTargets.Count >= targetCount)
			{
				break;
			}
		}

		return selectedTargets.ToArray();
	}
}

class CircleWaveGrowTowerAttackEffector : TowerAttackEffector
{
	public CircleWaveGrowTowerAttackEffector() : base(CIRCLE_WAVE_GROW) { }

	public override ITowerTargetable[] UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		int targetCount = tower.GetTowerAttackSettings().GetTowerAttackType().GetMaxTargets(target, tower);
		if (targetCount == 0)
		{
			return new ITowerTargetable[0];
		}

		//CircleCast, BoxCast, or CapsuleCast based on the tower attack size, then exclude targets within the tower attack distance
		Transform t = target.GetTowerTargetTracker();
		//By default, this method returns the results sorted by distance in ascending order (IE. the closest hit is first in the array)
		RaycastHit2D[] outer = Physics2D.CircleCastAll(
			new Vector2(t.position.x, t.position.y),
			(tower.GetAttackSize() + tower.GetAttackSizeDistance()) * attackPct,
			Vector2.zero);
#if DEBUG
		//If we're in Debug, draw the CircleCast
		Gizmos.color = Color.orangeRed;
		Gizmos.DrawWireSphere(t.position, (tower.GetAttackSize() + tower.GetAttackSizeDistance()) * attackPct);
#endif
		List<ITowerTargetable> selectedTargets = new List<ITowerTargetable>();

		//Always prioritize the original target
		foreach (RaycastHit2D hit in outer)
		{
			if (hit.collider.gameObject.GetEntityId() == target.GetTowerTargetTracker().gameObject.GetEntityId())
			{
				//If the original target is not a valid hit, do not include it and break
				if (hit.distance >= tower.GetAttackSizeDistance() * attackPct)
				{
					selectedTargets.Add(target);
				}
				else
				{
					break;
				}
			}
		}

		//Pick targets by distance (closest first)
		foreach (RaycastHit2D hit in outer)
		{
			ITowerTargetable hitTarget = hit.collider.gameObject.GetComponent<ITowerTargetable>();
			if (hitTarget == null)
			{
				continue;
			}

			if (hit.distance >= tower.GetAttackSizeDistance())
			{
				selectedTargets.Add(hitTarget);
			}

			if (selectedTargets.Count >= targetCount)
			{
				break;
			}
		}

		return selectedTargets.ToArray();
	}
}

class CircleWaveShrinkTowerAttackEffector : TowerAttackEffector
{
	public CircleWaveShrinkTowerAttackEffector() : base(CIRCLE_WAVE_SHRINK) { }

	public override ITowerTargetable[] UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		int targetCount = tower.GetTowerAttackSettings().GetTowerAttackType().GetMaxTargets(target, tower);
		if (targetCount == 0)
		{
			return new ITowerTargetable[0];
		}

		//CircleCast, BoxCast, or CapsuleCast based on the tower attack size, then exclude targets within the tower attack distance
		Transform t = target.GetTowerTargetTracker();
		//By default, this method returns the results sorted by distance in ascending order (IE. the closest hit is first in the array)
		RaycastHit2D[] outer = Physics2D.CircleCastAll(
			new Vector2(t.position.x, t.position.y),
			(tower.GetAttackSize() + tower.GetAttackSizeDistance()) * (1 - attackPct),
			Vector2.zero);
#if DEBUG
		//If we're in Debug, draw the CircleCast
		Gizmos.color = Color.orangeRed;
		Gizmos.DrawWireSphere(t.position, (tower.GetAttackSize() + tower.GetAttackSizeDistance()) * (1 - attackPct));
#endif
		List<ITowerTargetable> selectedTargets = new List<ITowerTargetable>();

		//Always prioritize the original target
		foreach (RaycastHit2D hit in outer)
		{
			if (hit.collider.gameObject.GetEntityId() == target.GetTowerTargetTracker().gameObject.GetEntityId())
			{
				//If the original target is not a valid hit, do not include it and break
				if (hit.distance >= tower.GetAttackSizeDistance() * (1 - attackPct))
				{
					selectedTargets.Add(target);
				}
				else
				{
					break;
				}
			}
		}

		//Pick targets by distance (closest first)
		foreach (RaycastHit2D hit in outer)
		{
			ITowerTargetable hitTarget = hit.collider.gameObject.GetComponent<ITowerTargetable>();
			if (hitTarget == null)
			{
				continue;
			}

			if (hit.distance >= tower.GetAttackSizeDistance())
			{
				selectedTargets.Add(hitTarget);
			}

			if (selectedTargets.Count >= targetCount)
			{
				break;
			}
		}

		return selectedTargets.ToArray();
	}
}

class SwipeTowerAttackEffector : TowerAttackEffector
{
	public SwipeTowerAttackEffector() : base(SWIPE) { }

	public override ITowerTargetable[] UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		int targetCount = tower.GetTowerAttackSettings().GetTowerAttackType().GetMaxTargets(target, tower);
		if (targetCount == 0)
		{
			return new ITowerTargetable[0];
		}

		//CircleCast, BoxCast, or CapsuleCast based on the tower attack size, then exclude targets within the tower attack distance
		Transform t = target.GetTowerTargetTracker();
		//By default, this method returns the results sorted by distance in ascending order (IE. the closest hit is first in the array)
		RaycastHit2D[] outer = Physics2D.BoxCastAll(
			new Vector2(t.position.x + (tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct), t.position.y),
			new Vector2(tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct, tower.GetAttackSize() + tower.GetAttackSizeDistance()),
			0f,
			Vector2.zero);
#if DEBUG
		//If we're in Debug, draw the BoxCast
		Gizmos.color = Color.orangeRed;
		Gizmos.DrawWireCube(t.position, new Vector3(tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct, tower.GetAttackSize() + tower.GetAttackSizeDistance(), 0f));
#endif
		List<ITowerTargetable> selectedTargets = new List<ITowerTargetable>();

		//Always prioritize the original target
		foreach (RaycastHit2D hit in outer)
		{
			if (hit.collider.gameObject.GetEntityId() == target.GetTowerTargetTracker().gameObject.GetEntityId())
			{
				//If the original target is not a valid hit, do not include it and break
				if (hit.distance >= tower.GetAttackSizeDistance())
				{
					selectedTargets.Add(target);
				}
				else
				{
					break;
				}
			}
		}

		//Pick targets by distance (closest first)
		foreach (RaycastHit2D hit in outer)
		{
			ITowerTargetable hitTarget = hit.collider.gameObject.GetComponent<ITowerTargetable>();
			if (hitTarget == null)
			{
				continue;
			}

			if (hit.distance >= tower.GetAttackSizeDistance())
			{
				selectedTargets.Add(hitTarget);
			}

			if (selectedTargets.Count >= targetCount)
			{
				break;
			}
		}

		return selectedTargets.ToArray();
	}
}

class SwipeReturnTowerAttackEffector : TowerAttackEffector
{
	public SwipeReturnTowerAttackEffector() : base(SWIPE_RETURN) { }

	public override ITowerTargetable[] UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		int targetCount = tower.GetTowerAttackSettings().GetTowerAttackType().GetMaxTargets(target, tower);
		if (targetCount == 0)
		{
			return new ITowerTargetable[0];
		}

		//CircleCast, BoxCast, or CapsuleCast based on the tower attack size, then exclude targets within the tower attack distance
		Transform t = target.GetTowerTargetTracker();
		//By default, this method returns the results sorted by distance in ascending order (IE. the closest hit is first in the array)
		RaycastHit2D[] outer = Physics2D.BoxCastAll(
			new Vector2(t.position.x + (tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct), t.position.y),
			new Vector2(tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct, tower.GetAttackSize() + tower.GetAttackSizeDistance()),
			0f,
			Vector2.zero);
#if DEBUG
		//If we're in Debug, draw the BoxCast
		Gizmos.color = Color.orangeRed;
		Gizmos.DrawWireCube(t.position, new Vector3(tower.GetAttackSize() + tower.GetAttackSizeDistance() * attackPct, tower.GetAttackSize() + tower.GetAttackSizeDistance(), 0f));
#endif
		List<ITowerTargetable> selectedTargets = new List<ITowerTargetable>();

		//Always prioritize the original target
		foreach (RaycastHit2D hit in outer)
		{
			if (hit.collider.gameObject.GetEntityId() == target.GetTowerTargetTracker().gameObject.GetEntityId())
			{
				//If the original target is not a valid hit, do not include it and break
				if (hit.distance >= tower.GetAttackSizeDistance())
				{
					selectedTargets.Add(target);
				}
				else
				{
					break;
				}
			}
		}

		//Pick targets by distance (closest first)
		foreach (RaycastHit2D hit in outer)
		{
			ITowerTargetable hitTarget = hit.collider.gameObject.GetComponent<ITowerTargetable>();
			if (hitTarget == null)
			{
				continue;
			}

			if (hit.distance >= tower.GetAttackSizeDistance())
			{
				selectedTargets.Add(hitTarget);
			}

			if (selectedTargets.Count >= targetCount)
			{
				break;
			}
		}

		return selectedTargets.ToArray();
	}
}