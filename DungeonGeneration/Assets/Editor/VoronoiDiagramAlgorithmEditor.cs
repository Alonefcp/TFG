using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoronoiDiagramAlgorithm))]
public class VoronoiDiagramAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty numberOfSeeds;
    SerializedProperty distanceFormula;
    SerializedProperty wallErosion;
    SerializedProperty randomShape;
    SerializedProperty addExtraPaths;
    SerializedProperty makeWiderPaths;
    SerializedProperty connectRegions;
    SerializedProperty wallThresholdSize;
    SerializedProperty floorThresholdSize;
    SerializedProperty randomSeeds;
    SerializedProperty seedMinDistance;
    SerializedProperty seedMaxDistance;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        numberOfSeeds = serializedObject.FindProperty("numberOfSeeds");
        distanceFormula = serializedObject.FindProperty("distanceFormula");
        wallErosion = serializedObject.FindProperty("wallErosion");
        randomShape = serializedObject.FindProperty("randomShape");
        addExtraPaths = serializedObject.FindProperty("addExtraPaths");
        makeWiderPaths = serializedObject.FindProperty("makeWiderPaths");
        connectRegions = serializedObject.FindProperty("connectRegions");
        wallThresholdSize = serializedObject.FindProperty("wallThresholdSize");
        floorThresholdSize = serializedObject.FindProperty("floorThresholdSize");
        randomSeeds = serializedObject.FindProperty("randomSeeds");
        seedMinDistance = serializedObject.FindProperty("seedMinDistance");
        seedMaxDistance = serializedObject.FindProperty("seedMaxDistance");
    }

    public override void OnInspectorGUI()
    {
        VoronoiDiagramAlgorithm vd = (VoronoiDiagramAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapWidth);
        EditorGUILayout.PropertyField(mapHeight);
        EditorGUILayout.PropertyField(numberOfSeeds);
        EditorGUILayout.PropertyField(distanceFormula);
        EditorGUILayout.PropertyField(wallErosion);
        EditorGUILayout.PropertyField(randomShape);
        EditorGUILayout.PropertyField(addExtraPaths);
        EditorGUILayout.PropertyField(makeWiderPaths);
        EditorGUILayout.PropertyField(connectRegions);
        EditorGUILayout.PropertyField(wallThresholdSize);
        EditorGUILayout.PropertyField(floorThresholdSize);
        EditorGUILayout.PropertyField(randomSeeds);
        if(!vd.RandomSeeds)
        {
            EditorGUILayout.PropertyField(seedMinDistance);
            EditorGUILayout.PropertyField(seedMaxDistance);
        }


        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
