using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonActionSequence : MonoBehaviour
{
    [Header("ButtonActionSequence")]
    public List<ButtonAction> actionSequence = new List<ButtonAction>();

    [Header("EventListener")]
    public UnityEvent onSequenceSuccess;
    public UnityEvent onSequenceFailed;

    private int currentStepIndex = 0;
    private bool isExecuting = false;

    private float stepTimer = 0f;
    private int currentPressCount = 0;
    private float timingProgress = 0f;
    private bool timingMovingForward = true;

    public float TimingProgress => timingProgress;

    public void StartSequence()
    {
        if (actionSequence == null || actionSequence.Count == 0)
        {
            return;
        }

        currentStepIndex = 0;
        isExecuting = true;
        PrepareStep(currentStepIndex);
    }

    private void PrepareStep(int index)
    {
        stepTimer = 0f;
        currentPressCount = 0;
        timingProgress = 0f;
        timingMovingForward = true;

        ButtonAction current = actionSequence[index];
    }

    private void Update()
    {
        if (!isExecuting)
        {
            return;
        }
        if (currentStepIndex >= actionSequence.Count)
        {
            return;
        }

        ButtonAction current = actionSequence[currentStepIndex];

        switch (current.ActionType)
        {
            case EButtonActionType.SinglePress:
                HandleSinglePress(current);
                break;
            case EButtonActionType.Hold:
                HandleHold(current);
                break;
            case EButtonActionType.RapidPress:
                HandleRapidPress(current);
                break;
            case EButtonActionType.Timing:
                HandleTiming(current);
                break;
        }
    }
    
    private void HandleSinglePress(ButtonAction action)
    {
        if (Input.GetKeyDown(action.Key))
        {
            AdvanceStep();
        }
    }

    private void HandleHold(ButtonAction action)
    {
        if (Input.GetKey(action.Key))
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= action.Duration)
            {
                AdvanceStep();
            }
        }
        else if (Input.GetKeyUp(action.Key) && stepTimer < action.Duration)
        {
            FailSequence();
        }
    }

    private void HandleRapidPress(ButtonAction action)
    {
        stepTimer += Time.deltaTime;

        if (Input.GetKeyDown(action.Key))
        {
            currentPressCount++;
            if (currentPressCount >= action.TargetPressCount)
            {
                AdvanceStep();
                return;
            }
        }

        if (stepTimer >= action.Duration)
        {
            FailSequence();
        }
    }

    private void HandleTiming(ButtonAction action)
    {
        if (timingMovingForward)
        {
            timingProgress += Time.deltaTime * action.TimingSpeed;
            if (timingProgress >= 1f)
            {
                timingProgress = 1f; timingMovingForward = false;
            }
        }
        else
        {
            timingProgress -= Time.deltaTime * action.TimingSpeed;
            if (timingProgress <= 0f) 
            { 
                timingProgress = 0f; timingMovingForward = true; 
            }
        }

        if (Input.GetKeyDown(action.Key))
        {
            if (timingProgress >= action.SuccessRangeStart && timingProgress <= action.SuccessRangeEnd)
            {
                AdvanceStep();
            }
            else
            {
                FailSequence();
            }
        }
    }

    private void AdvanceStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= actionSequence.Count)
        {
            isExecuting = false;
            onSequenceSuccess?.Invoke();
        }
        else
        {
            PrepareStep(currentStepIndex);
        }
    }

    private void FailSequence()
    {
        isExecuting = false;
        onSequenceFailed?.Invoke();
    }
}