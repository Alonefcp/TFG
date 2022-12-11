using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DiffusionLimitedAggregationAlgorithm))]
public class DiffusionLimitedAggregationAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty fillPercentage;
    SerializedProperty symmetryType;
    SerializedProperty brushSize;
    SerializedProperty seedSize;
    SerializedProperty useCentralAttractor;
    SerializedProperty eliminateSingleWallsCells;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        fillPercentage = serializedObject.FindProperty("fillPercentage");
        symmetryType = serializedObject.FindProperty("symmetryType");
        brushSize = serializedObject.FindProperty("brushSize");
        seedSize = serializedObject.FindProperty("seedSize");
        useCentralAttractor = serializedObject.FindProperty("useCentralAttractor");
        eliminateSingleWallsCells = serializedObject.FindProperty("eliminateSingleWallsCells");

    }

    public override void OnInspectorGUI()
    {
        DiffusionLimitedAggregationAlgorithm dla = (DiffusionLimitedAggregationAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapWidth);
        EditorGUILayout.PropertyField(mapHeight);
        EditorGUILayout.PropertyField(fillPercentage);
        EditorGUILayout.PropertyField(symmetryType);
        EditorGUILayout.PropertyField(brushSize);
        EditorGUILayout.PropertyField(seedSize);
        EditorGUILayout.PropertyField(useCentralAttractor);
        EditorGUILayout.PropertyField(eliminateSingleWallsCells);

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
