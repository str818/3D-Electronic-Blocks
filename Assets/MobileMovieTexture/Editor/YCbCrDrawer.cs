using UnityEngine;
using UnityEditor;
using System.Collections;

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
namespace MMT
{
    public class YCbCrDrawer : MaterialPropertyDrawer
    {

        static Color YCbCrToRGB(Vector4 yCbCr)
        {
            yCbCr.w = 1.0f;

            Vector4 YCbCr2R = new Vector4(1.1643828125f, 1.59602734375f, 0f, -.87078515625f);
            Vector4 YCbCr2G = new Vector4(1.1643828125f, -.81296875f, -.39176171875f, .52959375f);
            Vector4 YCbCr2B = new Vector4(1.1643828125f, 0f, 2.017234375f, -1.081390625f);

            return new Color(Vector4.Dot(yCbCr, YCbCr2R), Vector4.Dot(yCbCr, YCbCr2G), Vector4.Dot(yCbCr, YCbCr2B), 1.0f);
        }

        static Vector4 RGBToYCbCr(Color rgb)
        {
            rgb.a = 1.0f;

            Vector4 RGBToY = new Vector4(0.256789f, 0.50413f, 0.0979057f, 0.0625f);
            Vector4 RGBToCb = new Vector4(0.439215f, -0.367788f, -0.0714272f, 0.5f);
            Vector4 RGBToCr = new Vector4(-0.148223f, -0.290992f, 0.439215f, 0.5f);

            return new Vector4(Vector4.Dot(rgb, RGBToY), Vector4.Dot(rgb, RGBToCb), Vector4.Dot(rgb, RGBToCr), 1.0f);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // Setup
            EditorGUI.BeginChangeCheck();

			var cachedWidth = EditorGUIUtility.labelWidth;
			
			EditorGUIUtility.labelWidth = 120f;

            var color = EditorGUI.ColorField(position, label, YCbCrToRGB(prop.vectorValue));

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = RGBToYCbCr(color);
            }

			EditorGUIUtility.labelWidth = cachedWidth;
        }
    }
}
#endif //!UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2