using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public class ButtonActionSequence : ActionSequenceRunnerBase
    {
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
            float timer = 0f;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (Input.GetKeyDown(action.Key))
                {
                    onStepFinished?.Invoke(true);
                    yield break;
                }
                yield return null;
            }
            onStepFinished?.Invoke(false);
        }

        private IEnumerator ProcessHold(ButtonAction action, Action<bool> onStepFinished)
        {
            float timer = 0f;
            float holdTimer = 0f;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (Input.GetKey(action.Key))
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= action.HoldedDuration)
                    {
                        onStepFinished?.Invoke(true);
                        yield break;
                    }
                }
                else if (Input.GetKeyUp(action.Key) && holdTimer < action.Duration)
                {
                    onStepFinished?.Invoke(false);
                    yield break;
                }

                yield return null;
            }
            onStepFinished?.Invoke(false);
        }

        private IEnumerator ProcessRapidPress(ButtonAction action, Action<bool> onStepFinished)
        {
            float timer = 0f;
            int pressCount = 0;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (Input.GetKeyDown(action.Key))
                {
                    pressCount++;
                    if (pressCount >= action.TargetPressCount)
                    {
                        onStepFinished?.Invoke(true);
                        yield break;
                    }
                }
                yield return null;
            }

            onStepFinished?.Invoke(false);
        }

        private IEnumerator ProcessTiming(ButtonAction action, Action<bool> onStepFinished)
        {
            float timer = 0;
            float progress = 0f;
            bool movingForward = true;

            while (timer < action.Duration && !_isCancelled)
            {
                timer += Time.deltaTime;

                if (movingForward)
                {
                    progress += Time.deltaTime * action.TimingSpeed;
                    if (progress >= 1f) { progress = 1f; movingForward = false; }
                }
                else
                {
                    progress -= Time.deltaTime * action.TimingSpeed;
                    if (progress <= 0f) { progress = 0f; movingForward = true; }
                }

                if (Input.GetKeyDown(action.Key))
                {
                    bool isSuccess = progress >= action.SuccessRangeStart && progress <= action.SuccessRangeEnd;
                    onStepFinished?.Invoke(isSuccess);
                    yield break;
                }

                yield return null;
            }

            onStepFinished?.Invoke(false);
        }
    }
}