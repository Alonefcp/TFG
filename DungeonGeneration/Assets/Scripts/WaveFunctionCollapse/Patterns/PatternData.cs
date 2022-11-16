using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternData
{
    private Pattern pattern;
    private int frequency;
    private float frequencyRelative;
    private float frequencyRelativeLog2;

    public float FrequencyRelative { get => frequencyRelative; }
    public float FrequencyRelativeLog2 { get => frequencyRelativeLog2; }
    public Pattern Pattern { get => pattern; }

    public PatternData(Pattern _pattern)
    {
        pattern = _pattern;
    }

    public void AddToFrequency() { frequency++; }

    public void CalculateRealtiveFrequency(int total)
    {
        frequencyRelative = (float)frequency / total;
        frequencyRelativeLog2 = Mathf.Log(frequencyRelative, 2);
    }

    public bool CompareGrid(Direction dir, PatternData data)
    {
        return pattern.ComparePatternToAnotherPattern(dir, data.pattern);
    }
}
