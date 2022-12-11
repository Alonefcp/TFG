using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGeneration),true)]
public class DungeonGenerationEditor : Editor
{
    DungeonGeneration generator;

    SerializedProperty tilemapVisualizer;
    SerializedProperty playerController;
    SerializedProperty useRandomSeed;
    SerializedProperty seed;

    public virtual void OnEnable()
    {
        tilemapVisualizer = serializedObject.FindProperty("tilemapVisualizer");
        playerController = serializedObject.FindProperty("playerController");
        useRandomSeed = serializedObject.FindProperty("useRandomSeed");
        seed = serializedObject.FindProperty("seed");     
    }

    /// <summary>
    /// Creates a button on the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        generator = (DungeonGeneration)target;
        
        //base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(tilemapVisualizer);
        EditorGUILayout.PropertyField(playerController);
        EditorGUILayout.PropertyField(useRandomSeed);
        EditorGUILayout.PropertyField(seed);

        serializedObject.ApplyModifiedProperties();
    }

    protected void GenerateButton()
    {
        //True when the user press the button
        if (GUILayout.Button("Generate Dungeon"))
        {
            generator.GenerateDungeon();
        }
    }
}
