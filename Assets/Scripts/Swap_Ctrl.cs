using UnityEditor;
using UnityEngine;

public class Swap_Ctrl : MonoBehaviour
{
	public enum GAMEOBJECT_TYPE { XBOX, TRACKERS }
	public GAMEOBJECT_TYPE gameObjectType;
	[HideInInspector] public GAMEOBJECT_TYPE prev_gameObjectType;

	public GameObject xbox_gameobject_prefab;
	public GameObject trackers_gameobject_prefab;
	public GameObject gameobject_instance;
}

[CustomEditor(typeof(Swap_Ctrl))]
public class Swap_Ctrl_CustomEditor : Editor
{
	private bool showBaseOnInspectorGui;
	private new Swap_Ctrl target;

	public override void OnInspectorGUI()
	{
		target = (Swap_Ctrl)base.target;

		showBaseOnInspectorGui = EditorGUILayout.Toggle("Show Default Inspector", showBaseOnInspectorGui);

		if (showBaseOnInspectorGui)
		{
			base.OnInspectorGUI();
		}
		else
		{
			target.gameObjectType = (Swap_Ctrl.GAMEOBJECT_TYPE)EditorGUILayout.EnumPopup("GameObject type to use: ", target.gameObjectType);

			if (target.gameObjectType != target.prev_gameObjectType)
			{
				OnValidate();
				target.prev_gameObjectType = target.gameObjectType;
			}
		}
	}

	private void OnValidate()
	{
		if (target.gameObjectType == Swap_Ctrl.GAMEOBJECT_TYPE.XBOX)
		{
			DestroyImmediate(target.gameobject_instance);
			target.gameobject_instance = Instantiate(target.xbox_gameobject_prefab, target.transform);
			//target.xbox_gameobject.SetActive(true);
			//target.trackers_gameobject.SetActive(false);
		}
		else
		{
			DestroyImmediate(target.gameobject_instance);
			target.gameobject_instance = Instantiate(target.trackers_gameobject_prefab, target.transform);
			//target.trackers_gameobject.SetActive(true);
			//target.xbox_gameobject.SetActive(false);
		}
	}
}