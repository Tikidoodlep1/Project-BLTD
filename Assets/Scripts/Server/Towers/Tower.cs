using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TowerController))]
[RequireComponent(typeof(EdgeCollider2D))]
[DisallowMultipleComponent]
public class Tower : NetworkBehaviour, ITowerTargetable
{
	//====================================================================================================================================
	//														TOWER ATTRIBUTES
	//====================================================================================================================================
	[Header("Tower Attack Attributes")]
	[SerializeField]
	[Range(0.1f, 1000f)]
	private float _attackDuration;
	[SerializeField]
	[Range(0.1f, 1000f)]
	private float _attackInterval;

	[Header("Attack Range Attributes")]
	[SerializeField]
	[Range(0f, 1000f)]
	private float _attackRangeSizeX;
	[SerializeField]
	[Range(0f, 1000f)]
	private float _attackRangeSizeY;
	[SerializeField]
	[Range(0f, 1000f)]
	private float _attackRangeDistance;
	[SerializeField]
	[Range(5, 12)]
	[Tooltip("The number of collider verices in one half of the ellipse (Not used for rectanglular Attack Shape)")]
	private int _numPoints;

	[Header("Attack AOE Attributes")]
	[SerializeField]
	[Range(0f, 1000f)]
	[Tooltip("The attack size as a radius")]
	private float _attackSize;
	[SerializeField]
	[Range(0f, 1000f)]
	private float _attackDistance;

	[Header("From Registries")]
	[HideInInspector]
	public int attackTypeIndex;
	[HideInInspector]
	public int attackShapeIndex;
	[HideInInspector]
	public int attackEffectorIndex;
	[HideInInspector]
	public string attackType;
	[HideInInspector]
	public string attackShape;
	[HideInInspector]
	public string attackEffector;

	//Synchronized Variables
	private NetworkVariable<float> AttackRangeSizeX = new NetworkVariable<float>();
	private NetworkVariable<float> AttackRangeSizeY = new NetworkVariable<float>();
	private NetworkVariable<float> AttackRangeDistance = new NetworkVariable<float>();
	private NetworkVariable<float> AttackSize = new NetworkVariable<float>();
	private NetworkVariable<float> AttackDistance = new NetworkVariable<float>();
	private NetworkVariable<float> AttackDuration = new NetworkVariable<float>();
	private NetworkVariable<float> AttackInterval = new NetworkVariable<float>();
	private NetworkVariable<TowerAttack> AttackSettings = new NetworkVariable<TowerAttack>();

	//Server Only Variables
	private EdgeCollider2D attackTrigger;
	private bool IsAttacking;
	private TowerEffects AttackEffects;
	private List<ITowerTargetable> Attackables = new List<ITowerTargetable>();

	/*Tower Attacking Logical diagram (Promoting mods)
	 * Key: -> means call after previous, --> means call from 2nd previous after previous, ->@ means call repeatedly (on update, fixed update, or other applicable multi-call)
	 * 
	 * TowerController begins by determining that the Tower instance can attack and will attack.
	 * (Awake, Sync) Tower -> TowerAttack.SetColliderPoints(AttackSizeX, AttackSizeY, AttackDistance) -> TowerAttackShape.SetColliderPoints(AttackSizeX, AttackSizeY, AttackDistance) 
	 * (Server) TowerController -> ClientSimulateAttack(Transform Target, long attackStartTime) --> Tower.DoAttack(Transform Target, float AttackPct) -> TowerAttack.DoAttack(Transform Target, float AttackPct) -> TowerAttackType.SetHurtBoxPos(Transform Target, float AttackPct) -->@ TowerAttackEffector.MoveHurtBoxForAttack(Transform Target, float AttackPct)
	 * (Client) Tower Recieves ClientSimulateAttack RPC -> Tower.ClientSimulateAttack
	 */

	private void Awake()
	{
		if (this.attackTrigger == null || this.attackTrigger.points == null || this.attackTrigger.points.Length < 5 || this.attackTrigger.points.Length > 12)
		{
			this.SetColliderPoints();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Debug.Log("OnNetworkSpawn() Running! Attack Interval: " + _attackInterval);
		if (IsServer)
		{
			this.AttackRangeSizeX.Value = _attackRangeSizeX;
			this.AttackRangeSizeY.Value = _attackRangeSizeY;
			this.AttackRangeDistance.Value = _attackRangeDistance;
			this.AttackSize.Value = _attackSize;
			this.AttackDuration.Value = _attackDuration;
			this.AttackInterval.Value = _attackInterval;
			this.AttackSettings.Value = new TowerAttack(attackType, attackShape, attackEffector);
		}
		Debug.Log("OnNetworkSpawn() Finished! Attack Interval: " + this.AttackInterval.Value);
	}
	
	//protected override void OnNetworkPostSpawn()
	//{
	//	base.OnNetworkPostSpawn();

	//	Debug.Log("OnNetworkPostSpawn() Running! Attack Interval: " + _attackInterval);
	//	if (IsServer)
	//	{
	//		this.AttackRangeSizeX.Value = _attackRangeSizeX;
	//		this.AttackRangeSizeY.Value = _attackRangeSizeY;
	//		this.AttackRangeDistance.Value = _attackRangeDistance;
	//		this.AttackSize.Value = _attackSize;
	//		this.AttackDuration.Value = _attackDuration;
	//		this.AttackInterval.Value = _attackInterval;
	//		this.AttackSettings.Value = new TowerAttack(attackType, attackShape, attackEffector);
	//	}
	//	Debug.Log("OnNetworkPostSpawn() Finished! Attack Interval: " + this.AttackInterval.Value);
	//}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}

		ITowerTargetable obj;
		if (collision.gameObject != this.gameObject && collision.gameObject.TryGetComponent<ITowerTargetable>(out obj))
		{
			Attackables.Add(obj);
			Debug.Log("Added " + obj.GetTowerTargetTracker().gameObject.name + " to Attackables.");
		}
	}

	void OnTriggerStay2D(Collider2D collision)
	{
		if (collision != GetComponent<Collider2D>())
		{
			Debug.Log("Collider within range!");

			ITowerTargetable obj;
			if (collision.gameObject != this.gameObject && collision.gameObject.TryGetComponent<ITowerTargetable>(out obj))
			{
				Attackables.Add(obj);
				Debug.Log("ITowerTargetable " + obj.GetTowerTargetTracker().gameObject.name + " found in range.");
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}

		ITowerTargetable obj;
		if (other.gameObject.TryGetComponent<ITowerTargetable>(out obj))
		{
			Attackables.Remove(obj);
			Debug.Log("Removed " + obj.GetTowerTargetTracker().gameObject.name + " to Attackables.");
		}
	}

	public float GetAttackInterval()
	{
		return this.AttackInterval.Value;
	}

	public float GetAttackRangeX()
	{
		return this.AttackRangeSizeX.Value;
	}

	public float GetAttackRangeY()
	{
		return this.AttackRangeSizeY.Value;
	}

	public float GetAttackRangeDistance()
	{
		return this.AttackRangeDistance.Value;
	}

	public float GetAttackSize()
	{
		return this.AttackSize.Value;
	}

	public float GetAttackSizeDistance()
	{
		return this.AttackDistance.Value;
	}

	public TowerAttack GetTowerAttackSettings()
	{
		return this.AttackSettings.Value;
	}

	public IList<ITowerTargetable> GetTargetables()
	{
		return Attackables.AsReadOnlyList();
	}

	public virtual int GetHealth()
	{
		return 0;
	}

	public virtual bool IsCurrentlyValidTarget()
	{
		return true;
	}

	public virtual Transform GetTowerTargetTracker()
	{
		return this.gameObject.transform;
	}

	/// <summary>
	/// Calls this.AttackSettings.AttackTarget. Please see <see cref="TowerAttack.AttackTarget(ITowerTargetable, float)"/>
	/// </summary>
	/// <param name="target">The target that represents the primary target of the attack.</param>
	/// <param name="timeStart">The server ticks that the attack started at. Used to calculate the attack duration as a percentage [0.0, 1.0].</param>
	public virtual void AttackTarget(ITowerTargetable target, long timeStart)
	{
		if (IsServer)
		{
			this.IsAttacking = true;
			float attackPct = (float)(timeStart / TimeSpan.TicksPerSecond) / this.AttackDuration.Value;
			this.AttackSettings.Value.AttackTarget(target, this, attackPct);
			this.IsAttacking = false;
		}
		else
		{
			//IsClient
			this.CreateAttackFX(target);
		}

		Debug.Log("Trying to attack target " + target.GetTowerTargetTracker().gameObject.name + ". Time Start: " + timeStart);
	}

	public void OnTowerAttacked(TowerEffects attack)
	{
		//TODO
	}

	//==================================================================================================================================================\\
	//                                                                CLIENT ONLY                                                                       \\
	//==================================================================================================================================================\\

	public virtual void CreateAttackFX(ITowerTargetable target)
	{
		if (!IsClient)
		{
			return;
		}
	}

	//==================================================================================================================================================\\
	//                                                                EDITOR / DEBUG                                                                    \\
	//==================================================================================================================================================\\

	private void OnDrawGizmosSelected()
	{
		this.SetColliderPoints();

		Vector3 pos = this.gameObject.transform.position;
		Gizmos.color = new Color(0.7215f, 0.52549f, 0.04313f, 0.7f);
		Gizmos.DrawWireSphere(pos, this._attackRangeDistance);

		if (this.AttackSettings == null || this.AttackSettings.Value == null)
		{
			return;
		}

		Gizmos.color = Color.orangeRed;
		if (this.attackShape == TowerAttackShape.ELLIPSE)
		{
			float outerRadX = this._attackRangeSizeX / 2 + this._attackRangeDistance;
			float outerRadY = this._attackRangeSizeY / 2 + this._attackRangeDistance;

			//Outer circle
			Handles.DrawBezier(
				new Vector3(pos.x + outerRadX, pos.y, pos.z),
				new Vector3(pos.x, pos.y + outerRadY, pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(outerRadX, outerRadY / 2, 0)),
				this.gameObject.transform.TransformPoint(new Vector3(outerRadX / 2, outerRadY, 0)),
				new Color(0.75f, 0f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x - outerRadX, pos.y, pos.z),
				new Vector3(pos.x, pos.y + outerRadY, pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(-(outerRadX), outerRadY / 2, 0)),
				this.gameObject.transform.TransformPoint(new Vector3(-(outerRadX / 2), outerRadY, 0)),
				new Color(0.75f, 0f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x + outerRadX, pos.y, pos.z),
				new Vector3(pos.x, pos.y - outerRadY, pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(outerRadX, -(outerRadY / 2), 0)),
				this.gameObject.transform.TransformPoint(new Vector3(outerRadX / 2, -(outerRadY), 0)),
				new Color(0.75f, 0f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x - outerRadX, pos.y, pos.z),
				new Vector3(pos.x, pos.y - outerRadY, pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(-(outerRadX), -(outerRadY / 2), 0)),
				this.gameObject.transform.TransformPoint(new Vector3(-(outerRadX / 2), -(outerRadY), 0)),
				new Color(0.75f, 0f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);

			//Inner Circle
			Handles.DrawBezier(
				new Vector3(pos.x + (this._attackRangeSizeX / 2), pos.y, pos.z),
				new Vector3(pos.x, pos.y + (this._attackRangeSizeY / 2), pos.z),
				this.gameObject.transform.TransformPoint(new Vector3((this._attackRangeSizeX / 2), this._attackRangeSizeY / 4, 0)),
				this.gameObject.transform.TransformPoint(new Vector3(this._attackRangeSizeX / 4, (this._attackRangeSizeY / 2), 0)),
				new Color(0f, 0.75f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x - (this._attackRangeSizeX / 2), pos.y, pos.z),
				new Vector3(pos.x, pos.y + (this._attackRangeSizeY / 2), pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(-((this._attackRangeSizeX / 2)), this._attackRangeSizeY / 4, 0)),
				this.gameObject.transform.TransformPoint(new Vector3(-(this._attackRangeSizeX / 4), (this._attackRangeSizeY / 2), 0)),
				new Color(0f, 0.75f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x + (this._attackRangeSizeX / 2), pos.y, pos.z),
				new Vector3(pos.x, pos.y - (this._attackRangeSizeY / 2), pos.z),
				this.gameObject.transform.TransformPoint(new Vector3((this._attackRangeSizeX / 2), -(this._attackRangeSizeY / 4), 0)),
				this.gameObject.transform.TransformPoint(new Vector3(this._attackRangeSizeX / 4, -((this._attackRangeSizeY / 2)), 0)),
				new Color(0f, 0.75f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
			Handles.DrawBezier(
				new Vector3(pos.x - (this._attackRangeSizeX / 2), pos.y, pos.z),
				new Vector3(pos.x, pos.y - (this._attackRangeSizeY / 2), pos.z),
				this.gameObject.transform.TransformPoint(new Vector3(-((this._attackRangeSizeX / 2)), -(this._attackRangeSizeY / 4), 0)),
				this.gameObject.transform.TransformPoint(new Vector3(-(this._attackRangeSizeX / 4), -((this._attackRangeSizeY / 2)), 0)),
				new Color(0f, 0.75f, 0.75f, 0.7f), Texture2D.whiteTexture, 2f
			);
		}
		else if (this.attackShape == TowerAttackShape.RECTANGLE)
		{
			Gizmos.color = new Color(0f, 0.75f, 0.75f, 0.7f);
			Gizmos.DrawWireCube(pos, new Vector3(this._attackRangeSizeX, this._attackRangeSizeY, 0));
			Gizmos.color = new Color(0.75f, 0f, 0.75f, 0.7f);
			Gizmos.DrawWireCube(pos, new Vector3(this._attackRangeSizeX + (this._attackRangeDistance * 2), this._attackRangeSizeY + (this._attackRangeDistance * 2)));
		}
	}

	private void SetColliderPoints()
	{
		//if (this.AttackSettings == null || this.AttackSettings.Value == null)
		//{
		//	return;
		//}

		if (string.IsNullOrWhiteSpace(this.attackShape))
		{
			return;
		}

		if (this.attackTrigger == null)
		{
			this.attackTrigger = GetComponent<EdgeCollider2D>();
		}

		if (!this.attackTrigger.isTrigger)
		{
			this.attackTrigger.isTrigger = true;
		}

		if (this.attackTrigger.edgeRadius != this._attackRangeDistance / 2)
		{
			this.attackTrigger.edgeRadius = this._attackRangeDistance / 2;
		}

		Vector2[] points;

		if (this.attackShape == TowerAttackShape.ELLIPSE)
		{
			float sizeX = this._attackRangeSizeX / 2f;
			float sizeY = this._attackRangeSizeY / 2f;
			float ratio = (this._attackRangeSizeX / this._attackRangeSizeY);

			points = new Vector2[(_numPoints * 2) - 1];

			for (int i = 0; i < Mathf.CeilToInt(_numPoints / 2) + 1; i++)
			{
				float OuterX;
				float InnerX;
				float midPoints;

				//The three cardinal axes that we're plotting on
				if (i == 0 || i == _numPoints - 1)
				{
					OuterX = ((i * (sizeX + this._attackRangeDistance) * 2) / (_numPoints - 1)) - (sizeX + this._attackRangeDistance);
					InnerX = ((i * sizeX * 2) / (_numPoints - 1)) - sizeX;
				}
				else
				{
					midPoints = ((_numPoints - 3) / 2);
					OuterX = i * ((sizeX + this._attackRangeDistance) + ((i + 1) * ratio / midPoints)) * 2 / (_numPoints - 1) - ((sizeX + this._attackRangeDistance) + ((i + 1) * ratio / midPoints));
					InnerX = i * (sizeX + ((i + 1) * ratio / midPoints)) * 2 / (_numPoints - 1) - (sizeX + ((i + 1) * ratio / midPoints));
				}

				float OuterY = Mathf.Sqrt(
					(1f - (Mathf.Pow(OuterX, 2f) / Mathf.Pow(sizeX + this._attackRangeDistance, 2f))) * Mathf.Pow(sizeY + this._attackRangeDistance, 2f)
				);

				float InnerY = Mathf.Sqrt(
					(1f - (Mathf.Pow(InnerX, 2f) / Mathf.Pow(sizeX, 2f))) * Mathf.Pow(sizeY, 2f)
				);

				points[i] = (new Vector2((InnerX + OuterX) / 2f, (InnerY + OuterY) / 2f));
			}

			for (int i = 0; i < Mathf.CeilToInt(_numPoints / 2) + 1; i++)
			{
				points[_numPoints - 1 - i] = new Vector2(-points[i].x, points[i].y);
			}

			for (int i = 0; i < _numPoints - 1; i++)
			{
				points[points.Length - 1 - i] = new Vector2(points[i].x, -points[i].y);
			}
		}
		else
		{
			points = new Vector2[5];

			float sizeX = this._attackRangeSizeX / 2f;
			float sizeY = this._attackRangeSizeY / 2f;

			float OuterX = sizeX + this._attackRangeDistance;
			float InnerX = sizeX;
			float OuterY = sizeY + this._attackRangeDistance;
			float InnerY = sizeY;

			points[0] = new Vector2(-(InnerX + OuterX) / 2f, (InnerY + OuterY) / 2f);
			points[1] = new Vector2((InnerX + OuterX) / 2f, (InnerY + OuterY) / 2f);
			points[2] = new Vector2((InnerX + OuterX) / 2f, -(InnerY + OuterY) / 2f);
			points[3] = new Vector2(-(InnerX + OuterX) / 2f, -(InnerY + OuterY) / 2f);
			points[4] = new Vector2(-(InnerX + OuterX) / 2f, (InnerY + OuterY) / 2f);
		}

		this.attackTrigger.points = points;
	}
}
