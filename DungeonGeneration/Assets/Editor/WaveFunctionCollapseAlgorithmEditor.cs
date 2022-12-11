using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveFunctionCollapseAlgorithm))]
public class WaveFunctionCollapseAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty mapWidth, mapHeight;
    SerializedProperty addBorder;
    SerializedProperty forceMoreWalkableZones;
    SerializedProperty nWalkableZones;
    SerializedProperty maxWalkableZoneSize;
    SerializedProperty borderSprite;
    SerializedProperty playerSpriteSpawn;
    SerializedProperty tilesInfo;

    public override void OnEnable()
    {
        base.OnEnable();

        mapWidth = serializedObject.FindProperty("mapWidth");
        mapHeight = serializedObject.FindProperty("mapHeight");
        addBorder = serializedObject.FindProperty("addBorder");
        forceMoreWalkableZones = serializedObject.FindProperty("forceMoreWalkableZones");
        nWalkableZones = serializedObject.FindProperty("nWalkableZones");
        maxWalkableZoneSize = serializedObject.FindProperty("maxWalkableZoneSize");
        borderSprite = serializedObject.FindProperty("borderSprite");
        playerSpriteSpawn = serializedObject.FindProperty("playerSpriteSpawn");
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
        EditorGUILayout.PropertyField(forceMoreWalkableZones);
        if(wfc.MoreWalkableZones)
        {
            EditorGUILayout.PropertyField(nWalkableZones);
            EditorGUILayout.PropertyField(maxWalkableZoneSize);
        }
        EditorGUILayout.PropertyField(borderSprite);
        EditorGUILayout.PropertyField(playerSpriteSpawn);
        EditorGUILayout.PropertyField(tilesInfo);

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
