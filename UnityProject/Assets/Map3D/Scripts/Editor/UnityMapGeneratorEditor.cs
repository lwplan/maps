using UnityEditor;
using UnityEngine;

namespace maps.Unity
{
    [CustomEditor(typeof(UnityMapGenerator))]
    public class UnityMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnityMapGenerator gen = (UnityMapGenerator)target;

            if (GUILayout.Button("Generate Map"))
            {
                gen.GenerateAndRender();
            }
        }
    }
}
