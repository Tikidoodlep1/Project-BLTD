using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tower))]
public class TowerEditor : Editor
{
	//This is BAD, DON'T DO THIS
	private GameRegistries tempRegistry = new GameRegistries();

	private string[] attackTypeOptions;
	private string[] attackShapeOptions;
	private string[] attackEffectorOptions;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		tempRegistry.InitRegistry();

		if (attackTypeOptions == null || attackTypeOptions.Length <= 0 || string.IsNullOrWhiteSpace(attackTypeOptions[0]) || GUILayout.Button("Refresh Attack Types List"))
		{
			attackTypeOptions = new string[tempRegistry.TOWER_ATTACK_TYPES.GetKeys().Count];
			tempRegistry.TOWER_ATTACK_TYPES.GetKeys().CopyTo(attackTypeOptions, 0);
		}

		if (attackShapeOptions == null || attackShapeOptions.Length <= 0 || string.IsNullOrWhiteSpace(attackShapeOptions[0]) || GUILayout.Button("Refresh Attack Shapes List"))
		{
			attackShapeOptions = new string[tempRegistry.TOWER_ATTACK_SHAPES.GetKeys().Count];
			tempRegistry.TOWER_ATTACK_SHAPES.GetKeys().CopyTo(attackShapeOptions, 0);
		}

		if (attackEffectorOptions == null || attackEffectorOptions.Length <= 0 || string.IsNullOrWhiteSpace(attackEffectorOptions[0]) || GUILayout.Button("Refresh Attack Effectors List"))
		{
			attackEffectorOptions = new string[tempRegistry.TOWER_ATTACK_EFFECTORS.GetKeys().Count];
			tempRegistry.TOWER_ATTACK_EFFECTORS.GetKeys().CopyTo(attackEffectorOptions, 0);
		}

		if (this.target is Tower)
		{
			((Tower)this.target).attackTypeIndex = EditorGUILayout.Popup("Attack Type", ((Tower)this.target).attackTypeIndex, attackTypeOptions);
			((Tower)this.target).attackShapeIndex = EditorGUILayout.Popup("Attack Shape", ((Tower)this.target).attackShapeIndex, attackShapeOptions);
			((Tower)this.target).attackEffectorIndex = EditorGUILayout.Popup("Attack Effector", ((Tower)this.target).attackEffectorIndex, attackEffectorOptions);

			((Tower)this.target).attackType = attackTypeOptions[((Tower)this.target).attackTypeIndex];
			((Tower)this.target).attackShape = attackShapeOptions[((Tower)this.target).attackShapeIndex];
			((Tower)this.target).attackEffector = attackEffectorOptions[((Tower)this.target).attackEffectorIndex];
		}
	}
}