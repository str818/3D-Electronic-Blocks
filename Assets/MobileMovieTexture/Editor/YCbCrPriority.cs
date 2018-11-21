using UnityEngine;
using UnityEditor;
using System.Collections;

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
namespace MMT
{
    public class YCbCrPriority : MaterialPropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 30.0f;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position.height = 30.0f;

            // Setup
            EditorGUI.BeginChangeCheck();

			var cachedWidth = EditorGUIUtility.labelWidth;

			EditorGUIUtility.labelWidth = 120f;

            var vector = EditorGUI.Vector3Field(position, label, prop.vectorValue);

            if (EditorGUI.EndChangeCheck())
            {
                //Clamp it positive
                vector = Vector3.Max(Vector3.zero, vector);

                //Store length in w
                prop.vectorValue = new Vector4(vector.x, vector.y, vector.z, vector.magnitude);
            }

			EditorGUIUtility.labelWidth = cachedWidth;
        }
    }
}
#endif //!UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2