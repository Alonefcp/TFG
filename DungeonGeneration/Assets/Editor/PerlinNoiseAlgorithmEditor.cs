using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerlinNoiseAlgorithm))]
public class PerlinNoiseAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty noiseScale;
    SerializedProperty octaves;
    SerializedProperty persistance;
    SerializedProperty lacunarity;
    SerializedProperty fillPercent;
    SerializedProperty offset;
    SerializedProperty wallThresholdSize;
    SerializedProperty floorThresholdSize;
    SerializedProperty connectRegions;
    SerializedProperty connectionSize;
    SerializedProperty borderInterval;
    SerializedProperty borderOffset;
    SerializedProperty addBiggerBorders;
    SerializedProperty showPerlinNoiseTexture;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        noiseScale = serializedObject.FindProperty("noiseScale");
        octaves = serializedObject.FindProperty("octaves");
        persistance = serializedObject.FindProperty("persistance");
        lacunarity = serializedObject.FindProperty("lacunarity");
        fillPercent = serializedObject.FindProperty("fillPercent");
        offset = serializedObject.FindProperty("offset");
        wallThresholdSize = serializedObject.FindProperty("wallThresholdSize");
        floorThresholdSize = serializedObject.FindProperty("floorThresholdSize");
        connectRegions = serializedObject.FindProperty("connectRegions");
        connectionSize = serializedObject.FindProperty("connectionSize");
        borderInterval = serializedObject.FindProperty("borderInterval");
        borderOffset = serializedObject.FindProperty("borderOffset");
        addBiggerBorders = serializedObject.FindProperty("addBiggerBorders");
        showPerlinNoiseTexture = serializedObject.FindProperty("showPerlinNoiseTexture");
    }

    public override void OnInspectorGUI()
    {
        PerlinNoiseAlgorithm pl = (PerlinNoiseAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapWidth);
        EditorGUILayout.PropertyField(mapHeight);
        EditorGUILayout.PropertyField(noiseScale);
        EditorGUILayout.PropertyField(octaves);
        EditorGUILayout.PropertyField(persistance);
        EditorGUILayout.PropertyField(lacunarity);
        EditorGUILayout.PropertyField(fillPercent);
        EditorGUILayout.PropertyField(offset);
        EditorGUILayout.PropertyField(wallThresholdSize);
        EditorGUILayout.PropertyField(floorThresholdSize);
        EditorGUILayout.PropertyField(connectRegions);
        if(pl.ConnectRegions)
        {
            EditorGUILayout.PropertyField(connectionSize);
        }
        EditorGUILayout.PropertyField(addBiggerBorders);
        if(pl.AddBiggerBorders)
        {
            EditorGUILayout.PropertyField(borderInterval);
            EditorGUILayout.PropertyField(borderOffset);
        }
        EditorGUILayout.PropertyField(showPerlinNoiseTexture);

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
