using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BinarySpacePartitioningAlgorithm))]
public class BinarySpacePartitioningAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty spaceWidth, spaceHeight;
    SerializedProperty minRoomWidth, minRoomHeight; 

    SerializedProperty roomOffset;
    SerializedProperty minSplitPercent;
    SerializedProperty maxSplitPercent;

    SerializedProperty corridorsAlgorithm;
    SerializedProperty addSomeRemainingEdges;
    SerializedProperty widerCorridors;
    SerializedProperty corridorSize;

    SerializedProperty setSpecialRooms;

    public override void OnEnable()
    {
        base.OnEnable();

        spaceWidth = serializedObject.FindProperty("spaceWidth");
        spaceHeight = serializedObject.FindProperty("spaceHeight");
        minRoomWidth = serializedObject.FindProperty("minRoomWidth");
        minRoomHeight = serializedObject.FindProperty("minRoomHeight");
        roomOffset = serializedObject.FindProperty("roomOffset");
        minSplitPercent = serializedObject.FindProperty("minSplitPercent");
        maxSplitPercent = serializedObject.FindProperty("maxSplitPercent");
        corridorsAlgorithm = serializedObject.FindProperty("corridorsAlgorithm");
        addSomeRemainingEdges = serializedObject.FindProperty("addSomeRemainingEdges");
        widerCorridors = serializedObject.FindProperty("widerCorridors");
        corridorSize = serializedObject.FindProperty("corridorSize");
        setSpecialRooms = serializedObject.FindProperty("setSpecialRooms");

    }

    public override void OnInspectorGUI()
    {
        BinarySpacePartitioningAlgorithm bsp = (BinarySpacePartitioningAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(spaceWidth);
        EditorGUILayout.PropertyField(spaceHeight);
        EditorGUILayout.PropertyField(minRoomWidth);
        EditorGUILayout.PropertyField(minRoomHeight);
        EditorGUILayout.PropertyField(roomOffset);
        EditorGUILayout.PropertyField(minSplitPercent);
        EditorGUILayout.PropertyField(maxSplitPercent);

        EditorGUILayout.PropertyField(corridorsAlgorithm);
        if(bsp.CorridorsAlgorithmType == BinarySpacePartitioningAlgorithm.CorridorsAlgorithm.Delaunay_Prim_Astar)
        {
            EditorGUILayout.PropertyField(addSomeRemainingEdges);
        }
        
        EditorGUILayout.PropertyField(widerCorridors);
        if(bsp.WiderCorridors)
        {
            EditorGUILayout.PropertyField(corridorSize);
        }
        
        EditorGUILayout.PropertyField(setSpecialRooms);


        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
