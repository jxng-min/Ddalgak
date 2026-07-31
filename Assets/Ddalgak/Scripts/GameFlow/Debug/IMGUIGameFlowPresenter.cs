using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class IMGUIGameFlowPresenter : GameFlowPresenterBase
    {
        private const float PanelWidth = 420f;
        private const float PanelMargin = 20f;

        private GameRuntimeState _runtimeState;
        private EventData _currentEvent;
        private IReadOnlyList<ChoiceData> _currentChoices;
        private TurnResult _currentResult;
        private KingdomStatsSnapshot _displayedStats;
        private Vector2 _scrollPosition;
        private string _statusText;
        private bool _showChoices;
        private bool _showResult;
        private bool _isCancelled;

        public override IEnumerator ShowGameStart(GameRuntimeState state)
        {
            _runtimeState = state;
            _displayedStats = state.Stats.CreateSnapshot();
            _currentEvent = null;
            _currentChoices = null;
            _currentResult = null;
            _showChoices = false;
            _showResult = false;
            _isCancelled = false;
            _statusText = "게임 시작";
            yield break;
        }

        public override IEnumerator ShowEvent(EventData eventData)
        {
            _currentEvent = eventData;
            _currentChoices = null;
            _currentResult = null;
            _showChoices = false;
            _showResult = false;
            _statusText = "이벤트 확인";
            yield break;
        }

        public override IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices,
                                                Action<ChoiceData> onSelected)
        {
            _currentChoices = choices;
            _showChoices = true;
            _showResult = false;
            _statusText = "Q / Space / P 중 하나를 선택하십시오.";

            if (choices == null || choices.Count == 0)
            {
                _statusText = "선택지가 없습니다.";
                yield break;
            }

            yield return WaitUntilGameKeysReleased();

            while (!_isCancelled)
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    _statusText = "키보드를 찾지 못해 첫 번째 선택지를 선택했습니다.";
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
            _showChoices = false;
            _currentChoices = null;
            _statusText = "선택 완료";
            yield break;
        }

        public override IEnumerator ShowResult(TurnResult result)
        {
            _currentResult = result;
            _showChoices = false;
            _showResult = true;
            _statusText = "결과 확인";
            yield break;
        }

        public override IEnumerator AnimateStatChanges(KingdomStatsSnapshot before,
                                                        KingdomStatsSnapshot after,
                                                        StatModifier modifier)
        {
            _displayedStats = after;
            _statusText = "왕국 수치 반영 완료";
            yield break;
        }

        public override IEnumerator WaitForNextTurnInput()
        {
            _statusText = "Q / Space / P를 눌러 다음 이벤트로 진행하십시오.";
            yield return WaitUntilGameKeysReleased();

            while (!_isCancelled)
            {
                if (WasAnyGameKeyPressed())
                {
                    _statusText = "다음 이벤트 호출";
                    yield break;
                }

                yield return null;
            }
        }

        public override IEnumerator ShowProcedureLevelUp(int previousLevel, int currentLevel)
        {
            _statusText = $"절차 복잡도 상승: {previousLevel} → {currentLevel}";
            yield break;
        }

        public override IEnumerator ShowGameOver(EGameOverReason reason)
        {
            _showChoices = false;
            _statusText = $"게임 오버: {reason}";
            yield break;
        }

        public override IEnumerator ShowClear()
        {
            _showChoices = false;
            _statusText = "게임 클리어";
            yield break;
        }

        public override void Cancel()
        {
            _isCancelled = true;
            _statusText = "게임 흐름 중단";
        }

        private void OnGUI()
        {
            if (_runtimeState == null)
            {
                return;
            }

            Rect area = new(Screen.width - PanelWidth - PanelMargin,
                            PanelMargin,
                            PanelWidth,
                            Screen.height - PanelMargin * 2f);

            GUILayout.BeginArea(area, GUI.skin.box);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawGameState();
            DrawStats();
            DrawEvent();
            DrawChoices();
            DrawResult();
            DrawStatus();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawGameState()
        {
            GUILayout.Label("<b>[Game Flow]</b>", CreateRichTextStyle());
            GUILayout.Label($"State: {GetCurrentStateText()}");
            GUILayout.Label($"처리 이벤트: {_runtimeState.ProcessedEventCount}");
            GUILayout.Label($"절차 복잡도: {_runtimeState.ProcedureLevel}");
            GUILayout.Space(8f);
        }

        private void DrawStats()
        {
            GUILayout.Label("<b>[왕국 수치]</b>", CreateRichTextStyle());
            GUILayout.Label($"국고: {_displayedStats.Treasury}");
            GUILayout.Label($"민심: {_displayedStats.PublicSentiment}");
            GUILayout.Label($"안보: {_displayedStats.Security}");
            GUILayout.Space(8f);
        }

        private void DrawEvent()
        {
            if (_currentEvent == null)
            {
                return;
            }

            GUILayout.Label("<b>[이벤트]</b>", CreateRichTextStyle());
            GUILayout.Label(_currentEvent.title);
            GUILayout.Label(_currentEvent.description, GUI.skin.label);
            GUILayout.Space(8f);
        }

        private void DrawChoices()
        {
            if (!_showChoices || _currentChoices == null)
            {
                return;
            }

            GUILayout.Label("<b>[선택지]</b>", CreateRichTextStyle());
            foreach (ChoiceData choice in _currentChoices)
            {
                if (choice == null)
                {
                    continue;
                }

                GUILayout.Label($"[{choice.inputButton}] {choice.description}");
                if (!string.IsNullOrWhiteSpace(choice.changePreview))
                {
                    GUILayout.Label($"    {choice.changePreview}");
                }
            }

            GUILayout.Space(8f);
        }

        private void DrawResult()
        {
            if (!_showResult || _currentResult == null)
            {
                return;
            }

            GUILayout.Label("<b>[결과]</b>", CreateRichTextStyle());
            GUILayout.Label(_currentResult.ResultText);
            GUILayout.Label($"국고 {FormatModifier(_currentResult.FinalModifier.treasury)} / " +
                            $"민심 {FormatModifier(_currentResult.FinalModifier.publicSentiment)} / " +
                            $"안보 {FormatModifier(_currentResult.FinalModifier.security)}");
            GUILayout.Space(8f);
        }

        private void DrawStatus()
        {
            GUILayout.Label("<b>[상태]</b>", CreateRichTextStyle());
            GUILayout.Label(_statusText ?? string.Empty);
        }

        private string GetCurrentStateText()
        {
            GameFlowController controller = GetComponent<GameFlowController>();
            return controller != null ? controller.CurrentState.ToString() : "Unknown";
        }

        private static GUIStyle CreateRichTextStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                richText = true,
                wordWrap = true
            };
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

        private void SelectChoice(ChoiceData choice, Action<ChoiceData> onSelected)
        {
            _statusText = $"선택: {choice.inputButton} - {choice.description}";
            onSelected?.Invoke(choice);
        }

        private static bool IsAnyGameKeyPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.qKey.isPressed || keyboard.spaceKey.isPressed || keyboard.pKey.isPressed);
        }

        private static bool WasAnyGameKeyPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.qKey.wasPressedThisFrame ||
                    keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.pKey.wasPressedThisFrame);
        }

        private static string FormatModifier(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}
