using Unity.Netcode;
using UnityEngine;

public class GameRegistries : NetworkBehaviour
{
	public static GameRegistries Instance;

	private bool _initialized = false;
	public bool Initialized 
	{ 
		get
		{
			return _initialized;
		}
		private set
		{
			_initialized = value;
		}
	}

	//Register towers here for later use with objects - Maybe a GameObject can choose from a list of Towers in a dropdown menu (Have to create that custom field attribute)
	//public static readonly RegistryBase<Tower> TOWERS = new RegistryBase<Tower>("Towers");
	public readonly RegistryBase<TowerAttackType> TOWER_ATTACK_TYPES = new RegistryBase<TowerAttackType>("TowerAttackTypes");
	public readonly RegistryBase<TowerAttackShape> TOWER_ATTACK_SHAPES = new RegistryBase<TowerAttackShape>("TowerAttackShapes");
	public readonly RegistryBase<TowerAttackEffector> TOWER_ATTACK_EFFECTORS = new RegistryBase<TowerAttackEffector>("TowerAttackEffectors");

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (Instance == null)
		{
			Instance = this;
		}

		if (!Initialized)
		{
			InitRegistry();
			Initialized = true;
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (Instance != null)
		{
			Instance = null;
		}
	}

	public virtual void InitRegistry() 
	{
		if (Initialized)
		{
			return;
		}

		//Attack Types
		TowerAttackType SINGLE_TARGET = new SingleTargetTowerAttackType();
		TowerAttackType AOE = new AoeTowerAttackType();

		//Attack Shapes
		TowerAttackShape RECTANGLE = new TowerAttackShape(TowerAttackShape.RECTANGLE);
		TowerAttackShape ELLIPSE = new TowerAttackShape(TowerAttackShape.ELLIPSE);

		//Attack Effectors
		TowerAttackEffector INSTANT = new InstantTowerAttackEffector();
		TowerAttackEffector CIRCLE_WAVE_GROW = new CircleWaveGrowTowerAttackEffector();
		TowerAttackEffector CIRCLE_WAVE_SHRINK = new CircleWaveShrinkTowerAttackEffector();
		TowerAttackEffector SWIPE = new SwipeTowerAttackEffector();
		TowerAttackEffector SWIPE_RETURN = new SwipeReturnTowerAttackEffector();

		Debug.Log("Registering Game Registries! Is Server? " + NetworkManager.Singleton.IsServer + ", Is Client? " + NetworkManager.Singleton.IsClient + ", Is Host? " + NetworkManager.Singleton.IsHost);
	}
}