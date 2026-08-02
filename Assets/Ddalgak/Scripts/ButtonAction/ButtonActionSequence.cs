using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public class ButtonActionSequence : ActionSequenceRunnerBase
    {
        [SerializeField] private ButtonActionView actionView;

        private Coroutine _activeStepCoroutine;
        private bool _isCancelled;

        public override IEnumerator Run(IReadOnlyList<ButtonAction> buttonActions, Action<ActionSequenceResult> onCompleted)
        {
            _isCancelled = false;

            if (buttonActions == null || buttonActions.Count == 0)
            {
                onCompleted?.Invoke(ActionSequenceResult.Success(0));
                yield break;
            }

            int completedStepCount = 0;
            bool isOverallSuccess = true;
            int failedStepIndex = -1;

            for (int i = 0; i < buttonActions.Count; i++)
            {
                if (_isCancelled)
                {
                    onCompleted?.Invoke(ActionSequenceResult.Failure(completedStepCount, i));
                    yield break;
                }

                ButtonAction currentAction = buttonActions[i];
                bool isStepSuccess = false;

                _activeStepCoroutine = StartCoroutine(ExecuteActionStep(currentAction, success => isStepSuccess = success));
                yield return _activeStepCoroutine;
                _activeStepCoroutine = null;

                if (_isCancelled)
                {
                    onCompleted?.Invoke(ActionSequenceResult.Failure(completedStepCount, i));
                    yield break;
                }

                if (isStepSuccess)
                {
                    completedStepCount++;
                }
                else
                {
                    isOverallSuccess = false;
                    failedStepIndex = i;
                    break;
                }
            }

            ActionSequenceResult result = isOverallSuccess
                ? ActionSequenceResult.Success(completedStepCount)
                : ActionSequenceResult.Failure(completedStepCount, failedStepIndex);

            onCompleted?.Invoke(result);
        }

        public override void Cancel()
        {
            _isCancelled = true;

            if (_activeStepCoroutine != null)
            {
                StopCoroutine(_activeStepCoroutine);
                _activeStepCoroutine = null;
            }

            actionView?.Hide();
        }

        private IEnumerator ExecuteActionStep(ButtonAction action, Action<bool> onStepFinished)
        {
            if (action == null)
            {
                onStepFinished?.Invoke(true);
                yield break;
            }

            switch (action.ActionType)
            {
                case EButtonActionType.SinglePress:
                    yield return ProcessSinglePress(action, onStepFinished);
                    break;
                case EButtonActionType.Hold:
                    yield return ProcessHold(action, onStepFinished);
                    break;
                case EButtonActionType.RapidPress:
                    yield return ProcessRapidPress(action, onStepFinished);
                    break;
                case EButtonActionType.Timing:
                    yield return ProcessTiming(action, onStepFinished);
                    break;
                default:
                    onStepFinished?.Invoke(true);
                    break;
            }
        }

        private IEnumerator ProcessSinglePress(ButtonAction action, Action<bool> onStepFinished)
        {
            actionView?.Show(action.ActionType, action.Key);

            float timer = 0f;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (Keyboard.current[action.Key].wasPressedThisFrame)
                {
                    actionView?.Hide();
                    onStepFinished?.Invoke(true);
                    yield break;
                }
                yield return null;
            }

            actionView?.Hide();
            onStepFinished?.Invoke(false);
        }

        private IEnumerator ProcessHold(ButtonAction action, Action<bool> onStepFinished)
        {
            actionView?.Show(action.ActionType, action.Key, action.Duration);

            float timer = 0f;
            float holdTimer = 0f;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    break;
                }

                bool wrongKeyPressed = IsOtherActionKeyPressed(keyboard, action.Key);
                bool targetKeyReleased = keyboard[action.Key].wasReleasedThisFrame;

                if (wrongKeyPressed || targetKeyReleased)
                {
                    actionView?.SetHoldProgress(action.Key, 0f);
                    actionView?.Hide();
                    onStepFinished?.Invoke(false);
                    yield break;
                }

                if (keyboard[action.Key].isPressed)
                {
                    holdTimer += Time.deltaTime;
                    float progress = action.HoldedDuration > 0f
                        ? holdTimer / action.HoldedDuration
                        : 1f;
                    actionView?.SetHoldProgress(action.Key, progress);

                    if (holdTimer >= action.HoldedDuration)
                    {
                        actionView?.ResetHoldProgress();
                        actionView?.Hide();
                        onStepFinished?.Invoke(true);
                        yield break;
                    }
                }

                yield return null;
            }

            actionView?.ResetHoldProgress();
            actionView?.Hide();
            onStepFinished?.Invoke(false);
        }

        private static bool IsOtherActionKeyPressed(Keyboard keyboard, Key targetKey)
        {
            return targetKey != Key.P && keyboard[Key.P].wasPressedThisFrame ||
                   targetKey != Key.Q && keyboard[Key.Q].wasPressedThisFrame ||
                   targetKey != Key.Space && keyboard[Key.Space].wasPressedThisFrame;
        }

        private IEnumerator ProcessRapidPress(ButtonAction action, Action<bool> onStepFinished)
        {
            actionView?.Show(action.ActionType, action.Key);

            float timer = 0f;
            int pressCount = 0;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (Keyboard.current[action.Key].wasPressedThisFrame)
                {
                    pressCount++;
                    if (pressCount >= action.TargetPressCount)
                    {
                        actionView?.Hide();
                        onStepFinished?.Invoke(true);
                        yield break;
                    }
                }
                yield return null;
            }

            actionView?.Hide();
            onStepFinished?.Invoke(false);
        }

        private IEnumerator ProcessTiming(ButtonAction action, Action<bool> onStepFinished)
        {
            actionView?.Show(action.ActionType, action.Key, action.Duration);

            float timer = 0f;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;
                float progress = timer / action.Duration;

                if (Keyboard.current[action.Key].wasPressedThisFrame)
                {
                    bool isSuccess = progress >= action.SuccessRangeStart && progress <= action.SuccessRangeEnd;
                    actionView?.Hide();
                    onStepFinished?.Invoke(isSuccess);
                    yield break;
                }

                yield return null;
            }

            actionView?.Hide();
            onStepFinished?.Invoke(false);
        }
    }
}
