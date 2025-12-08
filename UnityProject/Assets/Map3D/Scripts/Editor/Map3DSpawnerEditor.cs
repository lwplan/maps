using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map3DSpawner))]
public class Map3DSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var spawner = (Map3DSpawner)target;

        if (GUILayout.Button("Generate 3D Map"))
        {
            spawner.Generate();
        }
    }
}