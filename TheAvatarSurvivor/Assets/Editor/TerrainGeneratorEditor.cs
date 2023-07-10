using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using DoDo.Terrain;

[CustomEditor(typeof(TerrainGenerator), true)]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TerrainGenerator terrainData = (TerrainGenerator)target;

        if (GUILayout.Button("Update Terrain"))
        {
            terrainData.EditorUpdate();

            EditorUtility.SetDirty(target);
        }
    }
}