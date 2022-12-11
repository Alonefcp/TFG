using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomWalkAlgorithm))]
public class RndomWalkAlgorithmEditor : DungeonGenerationEditor
{
    SerializedProperty startPosition;
    SerializedProperty startRandomlyEachIteration;
    SerializedProperty numberOfFloorPositions;
    SerializedProperty minSteps, maxSteps;
    SerializedProperty chanceToChangeDirection;
    SerializedProperty eliminateSingleWallsCells;
    SerializedProperty useEightDirections;
    SerializedProperty levyFlight;
    SerializedProperty levyFlightChance;
    SerializedProperty minStepLength;
    SerializedProperty maxStepLength;

    public override void OnEnable()
    {
        base.OnEnable();

        startPosition = serializedObject.FindProperty("startPosition");
        startRandomlyEachIteration = serializedObject.FindProperty("startRandomlyEachIteration");
        numberOfFloorPositions = serializedObject.FindProperty("numberOfFloorPositions");
        minSteps = serializedObject.FindProperty("minSteps");
        maxSteps = serializedObject.FindProperty("maxSteps");
        chanceToChangeDirection = serializedObject.FindProperty("chanceToChangeDirection");
        eliminateSingleWallsCells = serializedObject.FindProperty("eliminateSingleWallsCells");
        useEightDirections = serializedObject.FindProperty("useEightDirections");
        levyFlight = serializedObject.FindProperty("levyFlight");
        levyFlightChance = serializedObject.FindProperty("levyFlightChance");
        minStepLength = serializedObject.FindProperty("minStepLength");
        maxStepLength = serializedObject.FindProperty("maxStepLength");
    }

    public override void OnInspectorGUI()
    {
        RandomWalkAlgorithm randomWalkAlgorithm = (RandomWalkAlgorithm)target;

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(startPosition);
        EditorGUILayout.PropertyField(startRandomlyEachIteration);
        EditorGUILayout.PropertyField(numberOfFloorPositions);
        EditorGUILayout.PropertyField(minSteps);
        EditorGUILayout.PropertyField(maxSteps);
        EditorGUILayout.PropertyField(chanceToChangeDirection);
        EditorGUILayout.PropertyField(eliminateSingleWallsCells);
        EditorGUILayout.PropertyField(useEightDirections);
        EditorGUILayout.PropertyField(levyFlight);

        if(randomWalkAlgorithm.LevyFlight)
        {
            EditorGUILayout.PropertyField(levyFlightChance);
            EditorGUILayout.PropertyField(minStepLength);
            EditorGUILayout.PropertyField(maxStepLength);
        }

        serializedObject.ApplyModifiedProperties();

        GenerateButton();
    }
}
