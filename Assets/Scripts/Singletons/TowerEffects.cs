using Unity.Netcode;
using UnityEngine;

public class TowerEffects
{
	private TowerEffectType Type;
	private float Amount;
	private TowerStatus Status;

}

public enum TowerEffectType
{
	[InspectorName("Damage")]
	DAMAGE,
	[InspectorName("Heal")]
	HEAL,
	[InspectorName("Status")]
	STATUS
}

public enum TowerStatus
{
	[InspectorName("None")]
	NONE,
	[InspectorName("Weakened")]
	WEAKENED,
	[InspectorName("Empowered")]
	EMPOWERED,
	[InspectorName("Slowed")]
	SLOWED,
	[InspectorName("Hastened")]
	HASTENED,
	[InspectorName("Burned")]
	BURNED,
	[InspectorName("Stunned")]
	STUNNED
}