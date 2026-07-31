using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class DebugGameFlowPresenter : GameFlowPresenterBase
    {
        private bool _isCancelled;

        public override IEnumerator ShowGameStart(GameRuntimeState runtimeState)
        {
            _isCancelled = false;
            Debug.Log("[GameFlow] Game Start");
            LogStats(runtimeState.Stats.CreateSnapshot());
            yield break;
        }

        public override IEnumerator ShowEvent(EventData eventData)
        {
            Debug.Log($"[GameFlow] Event: {eventData.title}\n{eventData.description}");
            yield break;
        }

        public override IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices,
                                                Action<ChoiceData> onSelected)
        {
            if (choices == null || choices.Count == 0)
            {
                Debug.LogError("[GameFlow] There are no choices to display.");
                yield break;
            }

            foreach (ChoiceData choice in choices)
            {
                Debug.Log($"[GameFlow] {choice.inputButton}: {choice.description} ({choice.changePreview})");
            }

            yield return WaitUntilGameKeysReleased();

            while (!_isCancelled)
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    Debug.LogError("[GameFlow] Keyboard device was not found. The first choice is selected.");
                    onSelected?.Invoke(choices[0]);
                    yield break;
                }

                if (keyboard.qKey.wasPressedThisFrame &&
                    TryFindChoice(choices, EGameInputButton.Q, out ChoiceData qChoice))
                {
                    SelectChoice(qChoice, onSelected);
                    yield break;
                }

                if (keyboard.spaceKey.wasPressedThisFrame &&
                    TryFindChoice(choices, EGameInputButton.Space, out ChoiceData spaceChoice))
                {
                    SelectChoice(spaceChoice, onSelected);
                    yield break;
                }

                if (keyboard.pKey.wasPressedThisFrame &&
                    TryFindChoice(choices, EGameInputButton.P, out ChoiceData pChoice))
                {
                    SelectChoice(pChoice, onSelected);
                    yield break;
                }

                yield return null;
            }
        }

        public override IEnumerator HideChoices()
        {
            Debug.Log("[GameFlow] Hide Choices");
            yield break;
        }

        public override IEnumerator ShowResult(TurnResult result)
        {
            Debug.Log($"[GameFlow] Result: {result.ResultText}");
            Debug.Log($"[GameFlow] Modifier: Treasury {result.FinalModifier.treasury:+#;-#;0}, " +
                      $"Public Sentiment {result.FinalModifier.publicSentiment:+#;-#;0}, " +
                      $"Security {result.FinalModifier.security:+#;-#;0}");
            yield break;
        }

        public override IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                        KingdomStatsSnapshot after,
                                                        StatModifier modifier)
        {
            Debug.Log($"[GameFlow] Stats: " +
                      $"Treasury {before.Treasury} -> {after.Treasury}, " +
                      $"Public Sentiment {before.PublicSentiment} -> {after.PublicSentiment}, " +
                      $"Security {before.Security} -> {after.Security}");
            yield break;
        }

        public override IEnumerator WaitForNextTurnInput()
        {
            Debug.Log("[GameFlow] Press Q, Space, or P to continue.");
            yield return WaitUntilGameKeysReleased();

            while (!_isCancelled)
            {
                if (WasAnyGameKeyPressed())
                {
                    yield break;
                }

                yield return null;
            }
        }

        public override IEnumerator ShowProcedureLevelUp(int previousLevel, int currentLevel)
        {
            Debug.Log($"[GameFlow] Procedure Level: {previousLevel} -> {currentLevel}");
            yield break;
        }

        public override IEnumerator ShowGameOver(EGameOverReason reason)
        {
            Debug.Log($"[GameFlow] Game Over: {reason}");
            yield break;
        }

        public override IEnumerator ShowClear()
        {
            Debug.Log("[GameFlow] Clear");
            yield break;
        }

        public override void Cancel()
        {
            _isCancelled = true;
        }

        private IEnumerator WaitUntilGameKeysReleased()
        {
            while (!_isCancelled && IsAnyGameKeyPressed())
            {
                yield return null;
            }
        }

        private static bool TryFindChoice(IReadOnlyList<ChoiceData> choices,
                                          EGameInputButton inputButton,
                                          out ChoiceData result)
        {
            foreach (ChoiceData choice in choices)
            {
                if (choice != null && choice.inputButton == inputButton)
                {
                    result = choice;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static void SelectChoice(ChoiceData choice, Action<ChoiceData> onSelected)
        {
            Debug.Log($"[GameFlow] Selected: {choice.inputButton} - {choice.description}");
            onSelected?.Invoke(choice);
        }

        private static bool IsAnyGameKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.qKey.isPressed || keyboard.spaceKey.isPressed || keyboard.pKey.isPressed);
        }

        private static bool WasAnyGameKeyPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.qKey.wasPressedThisFrame ||
                    keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.pKey.wasPressedThisFrame);
        }

        private static void LogStats(KingdomStatsSnapshot stats)
        {
            Debug.Log($"[GameFlow] Stats: Treasury {stats.Treasury}, " +
                      $"Public Sentiment {stats.PublicSentiment}, " +
                      $"Security {stats.Security}");
        }
    }
}
