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

	//Registry Names
	public const string TOWER_ATTACK_TYPE_REGISTRY_NAME = "TowerAttackTypes";
	public const string TOWER_ATTACK_SHAPE_REGISTRY_NAME = "TowerAttackShapes";
	public const string TOWER_ATTACK_EFFECTOR_REGISTRY_NAME = "TowerAttackEffectors";

	//Registries
	public readonly RegistryBase<TowerAttackType> TOWER_ATTACK_TYPES = new RegistryBase<TowerAttackType>(TOWER_ATTACK_TYPE_REGISTRY_NAME);
	public readonly RegistryBase<TowerAttackShape> TOWER_ATTACK_SHAPES = new RegistryBase<TowerAttackShape>(TOWER_ATTACK_SHAPE_REGISTRY_NAME);
	public readonly RegistryBase<TowerAttackEffector> TOWER_ATTACK_EFFECTORS = new RegistryBase<TowerAttackEffector>(TOWER_ATTACK_EFFECTOR_REGISTRY_NAME);

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

		if (Instance == this)
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

		GameRegistries.Instance.TOWER_ATTACK_TYPES.Add(SINGLE_TARGET.name, SINGLE_TARGET);
		GameRegistries.Instance.TOWER_ATTACK_TYPES.Add(AOE.name, AOE);

		//Attack Shapes
		TowerAttackShape RECTANGLE = new TowerAttackShape(TowerAttackShape.RECTANGLE);
		TowerAttackShape ELLIPSE = new TowerAttackShape(TowerAttackShape.ELLIPSE);

		GameRegistries.Instance.TOWER_ATTACK_SHAPES.Add(RECTANGLE.name, RECTANGLE);
		GameRegistries.Instance.TOWER_ATTACK_SHAPES.Add(ELLIPSE.name, ELLIPSE);

		//Attack Effectors
		TowerAttackEffector INSTANT = new InstantTowerAttackEffector();
		TowerAttackEffector CIRCLE_WAVE_GROW = new CircleWaveGrowTowerAttackEffector();
		TowerAttackEffector CIRCLE_WAVE_SHRINK = new CircleWaveShrinkTowerAttackEffector();
		TowerAttackEffector SWIPE = new SwipeTowerAttackEffector();
		TowerAttackEffector SWIPE_RETURN = new SwipeReturnTowerAttackEffector();

		GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.Add(INSTANT.name, INSTANT);
		GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.Add(CIRCLE_WAVE_GROW.name, CIRCLE_WAVE_GROW);
		GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.Add(CIRCLE_WAVE_SHRINK.name, CIRCLE_WAVE_SHRINK);
		GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.Add(SWIPE.name, SWIPE);
		GameRegistries.Instance.TOWER_ATTACK_EFFECTORS.Add(SWIPE_RETURN.name, SWIPE_RETURN);

		Debug.Log("Registering Game Registries! Is Server? " + NetworkManager.Singleton.IsServer + ", Is Client? " + NetworkManager.Singleton.IsClient + ", Is Host? " + NetworkManager.Singleton.IsHost);
	}
}