using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(EnvironmentObject))]
public class EnvironmentObjectEditor : Editor
{
	bool hasPrefabPath = false;

	public override void OnInspectorGUI()
	{
		GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.label);
		Color myStyleColor = Color.red;
		myFoldoutStyle.fontStyle = FontStyle.Bold;
		myFoldoutStyle.normal.textColor = myStyleColor;
		myFoldoutStyle.onNormal.textColor = myStyleColor;
		myFoldoutStyle.hover.textColor = myStyleColor;
		myFoldoutStyle.onHover.textColor = myStyleColor;
		myFoldoutStyle.focused.textColor = myStyleColor;
		myFoldoutStyle.onFocused.textColor = myStyleColor;
		myFoldoutStyle.active.textColor = myStyleColor;
		myFoldoutStyle.onActive.textColor = myStyleColor;

		EnvironmentObject thisObject = ((EnvironmentObject)target);

		if (thisObject.GetComponent<EnvironmentEngine>() == null)
		{
			if (thisObject.mPrefabPath == null || thisObject.mPrefabPath == "")
			{
				EditorGUILayout.LabelField("This object has no associated Prefab -- cannot load.", myFoldoutStyle);
			}


			Rigidbody thisRigidbody = thisObject.GetComponent<Rigidbody>();
			Rigidbody2D thisRigidbody2D = thisObject.GetComponent<Rigidbody2D>();

			if (thisObject.GetComponentInChildren<Collider>() != null || thisObject.GetComponentInChildren<Collider2D>() != null)
			{
				if (thisRigidbody == null && thisRigidbody2D == null)
				{
					EditorGUILayout.LabelField("This object has no associated Rigidbody(2D) -- no collisions.", myFoldoutStyle);
				}
			}
		}

		//base.OnInspectorGUI();
	}
}