using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CellularAutomataAlgorithm))]
public class CellularAutomataAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty iterations;
    SerializedProperty fillPercent;
    SerializedProperty neighborhood;
    SerializedProperty mooreRule;
    SerializedProperty vonNeummannRule;
    SerializedProperty connectRegions;
    SerializedProperty connectionSize;
    SerializedProperty wallThresholdSize;
    SerializedProperty floorThresholdSize;
    SerializedProperty useHilbertCurve;
    SerializedProperty order;
    SerializedProperty minOffsetX, maxOffsetX;
    SerializedProperty minOffsetY, maxOffsetY;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        iterations = serializedObject.FindProperty("iterations");
        fillPercent = serializedObject.FindProperty("fillPercent");
        neighborhood = serializedObject.FindProperty("neighborhood");
        mooreRule = serializedObject.FindProperty("mooreRule");
        vonNeummannRule = serializedObject.FindProperty("vonNeummannRule");
        connectRegions = serializedObject.FindProperty("connectRegions");
        connectionSize = serializedObject.FindProperty("connectionSize");
        wallThresholdSize = serializedObject.FindProperty("wallThresholdSize");
        floorThresholdSize = serializedObject.FindProperty("floorThresholdSize");
        useHilbertCurve = serializedObject.FindProperty("useHilbertCurve");
        order = serializedObject.FindProperty("order");
        minOffsetX = serializedObject.FindProperty("minOffsetX");
        maxOffsetX = serializedObject.FindProperty("maxOffsetX");
        minOffsetY = serializedObject.FindProperty("minOffsetY");
        maxOffsetY = serializedObject.FindProperty("maxOffsetY");
    }

    public override void OnInspectorGUI()
    {
        CellularAutomataAlgorithm ca = (CellularAutomataAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapWidth);
        EditorGUILayout.PropertyField(mapHeight);
        EditorGUILayout.PropertyField(iterations);
        EditorGUILayout.PropertyField(fillPercent);
        EditorGUILayout.PropertyField(neighborhood);
        if(ca.NeighborhoodType == CellularAutomataAlgorithm.Neighborhood.Moore)
        {
            EditorGUILayout.PropertyField(mooreRule);
        }
        else
        {
            EditorGUILayout.PropertyField(vonNeummannRule);
        }
       
        EditorGUILayout.PropertyField(connectRegions);
        if(ca.ConnectRegions)
        {
            EditorGUILayout.PropertyField(connectionSize);
        }
        EditorGUILayout.PropertyField(wallThresholdSize);
        EditorGUILayout.PropertyField(floorThresholdSize);
        EditorGUILayout.PropertyField(useHilbertCurve);
        if(ca.UseHilbertCurve)
        {
            EditorGUILayout.PropertyField(order);
            EditorGUILayout.PropertyField(minOffsetX);
            EditorGUILayout.PropertyField(maxOffsetX);
            EditorGUILayout.PropertyField(minOffsetY);
            EditorGUILayout.PropertyField(maxOffsetY);
        }
        

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
