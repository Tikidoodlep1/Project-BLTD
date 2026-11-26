using Unity.Netcode;

public class TowerAttackShape : INetworkSerializable
{
	public const string RECTANGLE = "RECTANGLE";
	public const string ELLIPSE = "ELLIPSE";

	public string name;

	public TowerAttackShape(string name)
	{
		name.Replace(" ", "_");
		name.ToUpper();

		this.name = name;
		GameRegistries.Instance.TOWER_ATTACK_SHAPES.Add(name, this);
	}

	public virtual void NetworkSerialize<T>(BufferSerializer<T> buf) where T : IReaderWriter
	{
		buf.SerializeValue(ref name);
	}
}