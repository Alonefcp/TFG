using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveFunctionCollapseAlgorithm))]
public class WaveFunctionCollapseAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty addBorder;
    SerializedProperty useBacktracking;
    SerializedProperty forceMoreWalkableZones;
    SerializedProperty nWalkableZones;
    SerializedProperty maxWalkableZoneSize;
    SerializedProperty borderSprite;
    SerializedProperty floorSpriteIndex;
    SerializedProperty tilesInfo;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        addBorder = serializedObject.FindProperty("addBorder");
        useBacktracking = serializedObject.FindProperty("useBacktracking");
        forceMoreWalkableZones = serializedObject.FindProperty("forceMoreWalkableZones");
        nWalkableZones = serializedObject.FindProperty("nWalkableZones");
        maxWalkableZoneSize = serializedObject.FindProperty("maxWalkableZoneSize");
        borderSprite = serializedObject.FindProperty("borderSprite");
        floorSpriteIndex = serializedObject.FindProperty("floorSpriteIndex");
        tilesInfo = serializedObject.FindProperty("tilesInfo");
    }

    public override void OnInspectorGUI()
    {
        WaveFunctionCollapseAlgorithm wfc = (WaveFunctionCollapseAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapWidth);
        EditorGUILayout.PropertyField(mapHeight);
        EditorGUILayout.PropertyField(addBorder);
        EditorGUILayout.PropertyField(useBacktracking);
        EditorGUILayout.PropertyField(forceMoreWalkableZones);
        if(wfc.MoreWalkableZones)
        {
            EditorGUILayout.PropertyField(nWalkableZones);
            EditorGUILayout.PropertyField(maxWalkableZoneSize);
        }
        EditorGUILayout.PropertyField(borderSprite);
        EditorGUILayout.PropertyField(floorSpriteIndex);
        EditorGUILayout.PropertyField(tilesInfo);

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
