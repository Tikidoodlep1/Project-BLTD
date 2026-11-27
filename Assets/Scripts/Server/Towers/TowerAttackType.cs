using Unity.Netcode;

public abstract class TowerAttackType : INetworkSerializable, IRegistryType
{
	public const string SINGLE_TARGET = "SINGLE_TARGET";
	public const string AOE = "AOE";

	public string name;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="name">The registry name for the TowerAttackType entry. By default, there are "SINGLE_TARGET" and "AOE" entries in ther registry.</param>
	public TowerAttackType(string name)
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
	/// <para>Use this method to return the maximum number of targets that the TowerAttackEffector for the TowerAttack can return at once. </para>
	/// 
	/// <para>In this context, 0 means that the tower cannot attack a target. -1 means that the TowerAttackEffector can return unlimited targets.</para>
	/// </summary>
	/// <param name="target">The ITowerTargetable that is the primary target of the attack.</param>
	/// <param name="tower">The tower performing the attack.</param>
	/// <returns></returns>
	public abstract int GetMaxTargets(ITowerTargetable target, Tower tower);
}

class SingleTargetTowerAttackType : TowerAttackType
{
	public SingleTargetTowerAttackType() : base(SINGLE_TARGET) { }

	public override int GetMaxTargets(ITowerTargetable target, Tower tower)
	{
		return 1;
	}
}

class AoeTowerAttackType : TowerAttackType
{
	public AoeTowerAttackType() : base(AOE) { }

	public override int GetMaxTargets(ITowerTargetable target, Tower tower)
	{
		return -1;
	}
}