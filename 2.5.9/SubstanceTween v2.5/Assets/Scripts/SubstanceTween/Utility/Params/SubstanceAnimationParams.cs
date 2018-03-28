using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parameters for handling animation
/// </summary>
[System.Serializable]
public class SubstanceAnimationParams
{
    public AnimationCurve substanceCurve = AnimationCurve.Linear(0, 0, 0, 0); // main curve for editing keyframes
    public AnimationCurve substanceCurveBackup = AnimationCurve.Linear(0, 0, 0, 0);
    public List<float> keyFrameTimes = new List<float>(); // time between keyframes
    public bool cacheSubstance = true, substanceLerp, animateOutputParameters = true, animateBackwards;
    public int defaultSubstanceIndex, keyFrames, currentKeyframeIndex;
    public float desiredAnimationTime;
    public float totalAnimationLength;
    public float defaultAnimationTime = 5, currentAnimationTime = 0, lerp, lerpCalc, keyframeSum = 0, curveFloat, animationTimeRestartEnd, currentKeyframeAnimationTime;
    public enum AnimationType { Loop, BackAndForth };
    public AnimationType animationType;
    public bool hideNonAnimatingVariables = true; //hides non changing variables when animating
}
