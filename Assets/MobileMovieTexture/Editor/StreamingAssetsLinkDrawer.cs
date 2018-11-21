using System;
using UnityEngine;
using UnityEditor;

namespace MMT
{
	[CustomPropertyDrawer(typeof(StreamingAssetsLinkAttribute))]
	public class StreaminAssetsLinkDrawer : PropertyDrawer 
	{
        public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label)
	    {
	        var linkAttribute = attribute as StreamingAssetsLinkAttribute;

            var currentObject = AssetDatabase.LoadAssetAtPath("Assets/StreamingAssets/" + prop.stringValue, linkAttribute.LinkType);

			EditorGUI.BeginChangeCheck();

			var newObject = EditorGUI.ObjectField(pos, linkAttribute.Label, currentObject, linkAttribute.LinkType, false);

			if (EditorGUI.EndChangeCheck())
			{
	            var path = AssetDatabase.GetAssetPath(newObject);

				if (path.Contains("Assets/StreamingAssets/") || string.IsNullOrEmpty(path))
	            {
					path = path.Replace("Assets/StreamingAssets/", "");

					//Undo.RecordObjects(targets, "Change movie reference");

	                prop.stringValue = path;
	            }
	            else
	            {
					Debug.LogError("Link must be in the StreamingAssets directory, path " + path + " is not ");
	            }
	        }
		}
	}
}
