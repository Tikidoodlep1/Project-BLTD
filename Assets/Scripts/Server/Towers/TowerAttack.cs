using System;
using Unity.Netcode;
using UnityEngine;

public class TowerAttack : INetworkSerializable, IEquatable<TowerAttack>
{
	private TowerAttackType Type;
	private TowerAttackShape Shape;
	private TowerAttackEffector Effector;

	public TowerAttack() {}

	public TowerAttack(TowerAttackType type, TowerAttackShape shape, TowerAttackEffector effector)
	{
		this.Type = type;
		this.Shape = shape;
		this.Effector = type == GameRegistries.Instance.TOWER_ATTACK_TYPES.GetValue(TowerAttackType.SINGLE_TARGET) ? GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.GetValue(TowerAttackEffector.INSTANT) : effector;
	}

	public TowerAttack(string type, string shape, string effector)
	{
		this.Type = GameRegistries.Instance.TOWER_ATTACK_TYPES.GetValue(type);
		this.Shape = GameRegistries.Instance.TOWER_ATTACK_SHAPES.GetValue(shape);
		this.Effector = GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.GetValue(effector);

		if (this.Type == null)
		{
			Debug.LogError((NetworkManager.Singleton.IsHost ? "[HOST]: " : NetworkManager.Singleton.IsClient ? "[CLIENT]: " : "[SERVER]: ")
			+ "TowerAttackType is null! Trying to find type " + type);
		}
		if (this.Shape == null)
		{
			Debug.LogError((NetworkManager.Singleton.IsHost ? "[HOST]: " : NetworkManager.Singleton.IsClient ? "[CLIENT]: " : "[SERVER]: ")
			+ "TowerAttackShape is null! Trying to find shape " + shape);
		}
		if (this.Effector == null)
		{
			Debug.LogError((NetworkManager.Singleton.IsHost ? "[HOST]: " : NetworkManager.Singleton.IsClient ? "[CLIENT]: " : "[SERVER]: ")
			+ "TowerAttackEffector is null! Trying to find effector " + effector);
		}
	}

	public bool Equals(TowerAttack other)
	{
		return other.Type == this.Type && other.Shape == this.Shape && other.Effector == this.Effector;
	}
	
	public TowerAttackType GetTowerAttackType()
	{
		return Type;
	}

	public TowerAttackShape GetTowerAttackShape()
	{
		return Shape;
	}

	public TowerAttackEffector GetTowerAttackEffector()
	{
		return Effector;
	}

	public void SetTowerAttackType(TowerAttackType type)
	{
		this.Type = type;
	}

	public void SetTowerAttackShape(TowerAttackShape shape)
	{
		this.Shape = shape;
	}

	public void SetTowerAttackEffector(TowerAttackEffector effector)
	{
		this.Effector = effector;
	}

	/// <summary>
	/// See <see cref="TowerAttackEffector.StartAttack(ITowerTargetable, Tower, float)"/>
	/// </summary>
	/// <param name="target">The target that represents the primary target of the attack.</param>
	/// <param name="attackPct">The attack duration as a percent [0.0, 1.0].</param>
	public void AttackTarget(ITowerTargetable target, Tower tower, float attackPct)
	{
		this.Effector.StartAttack(target, tower, attackPct);
	}
	
	public void UpdateAttack(ITowerTargetable target, Tower tower, float attackPct)
	{
		this.Effector.UpdateAttack(target, tower, attackPct);
	}
	
	public virtual void NetworkSerialize<T>(BufferSerializer<T> buf) where T : IReaderWriter
	{
		if (this.Type != null)
		{
			this.Type.NetworkSerialize(buf);
		}
		else
		{
			Debug.LogWarning("Cannot serialize a null TowerAttackType! Setting to the default TowerAttackType " + TowerAttackType.SINGLE_TARGET);
			this.Type = GameRegistries.Instance.TOWER_ATTACK_TYPES.GetValue(TowerAttackType.SINGLE_TARGET);
			this.Type.NetworkSerialize(buf);
		}

		if (this.Shape != null)
		{
			this.Shape.NetworkSerialize(buf);
		}
		else
		{
			Debug.LogWarning("Cannot serialize a null TowerAttackShape! Setting to the default TowerAttackShape " + TowerAttackShape.RECTANGLE);
			this.Shape = GameRegistries.Instance.TOWER_ATTACK_SHAPES.GetValue(TowerAttackShape.RECTANGLE);
			this.Shape.NetworkSerialize(buf);
		}

		if (this.Effector != null)
		{
			this.Effector.NetworkSerialize(buf);
		}
		else
		{
			Debug.LogWarning("Cannot serialize a null TowerAttackEffector! Setting to the default TowerAttackEffector " + TowerAttackEffector.INSTANT);
			this.Effector = GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.GetValue(TowerAttackEffector.INSTANT);
			this.Effector.NetworkSerialize(buf);
		}
	}
}