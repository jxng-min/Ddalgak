using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ButtonAction
{
    [SerializeField]
    private string actionID;
    [SerializeField]
    private EButtonActionType actionType;
    [SerializeField]
    private KeyCode key;

    [Header("제한 시간")]
    [SerializeField]
    private float duration;

    [Header("홀드 시간")]
    [SerializeField]
    private float holdDuration;

    [Header("목표 횟수")]
    [SerializeField]
    private int targetPressCount;

    [Header("Timing 설정")]
    [SerializeField]
    private float timingSpeed;
    [SerializeField]
    [Range(0f, 1f)]
    private float successRangeStart;
    [SerializeField]
    [Range(0f, 1f)]
    private float successRangeEnd;

    public string ActionID => actionID;
    public EButtonActionType ActionType => actionType;
    public KeyCode Key => key;
    public float Duration => duration;
    public float HoldedDuration => holdDuration;
    public int TargetPressCount => targetPressCount;
    public float TimingSpeed => timingSpeed;
    public float SuccessRangeStart => successRangeStart;
    public float SuccessRangeEnd => successRangeEnd;
}
