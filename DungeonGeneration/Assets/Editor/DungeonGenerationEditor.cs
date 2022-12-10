using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGeneration),true)]
public class DungeonGenerationEditor : Editor
{
    DungeonGeneration generator;

    private void Awake()
    {
        generator = (DungeonGeneration)target;
    }

    /// <summary>
    /// Creates a button on the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //true when the user press the button
        if(GUILayout.Button("Generate Dungeon"))
        {
            generator.GenerateDungeon();
        }
    }
}
