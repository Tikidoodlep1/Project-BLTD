using Unity.Netcode;

public class TowerAttackShape : INetworkSerializable, IRegistryType
{
	public const string RECTANGLE = "RECTANGLE";
	public const string ELLIPSE = "ELLIPSE";

	public string name;

	public TowerAttackShape(string name)
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
}