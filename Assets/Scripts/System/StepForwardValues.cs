using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StepForwardValues", menuName = "Value/New StepForwardValues", order = 2)]

public class StepForwardValues : ScriptableObject
{
    public float distance;
    public float duration;
    public float angle;
}
