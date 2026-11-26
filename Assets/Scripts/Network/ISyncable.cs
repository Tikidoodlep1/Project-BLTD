public abstract class ISyncable
{
	public bool IsDirty { get; set; }

	public abstract void Sync();
	public void MarkDirty()
	{
 		this.IsDirty = true;
	}

	public void MarkClean()
	{
		this.IsDirty = false;
	}

	public virtual bool ShouldSync()
	{
		return this.IsDirty;
	}
}
