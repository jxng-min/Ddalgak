using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class IMGUIGameFlowPresenter : GameFlowPresenterBase
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float PanelWidth = 1080f;
        private const float PanelMargin = 30f;
        private const int BodyFontSize = 24;
        private const int HeaderFontSize = 30;

        private GameRuntimeState _runtimeState;
        private EventData _currentEvent;
        private int _currentEventWeek;
        private int _currentEventSlotIndex;
        private EEventType _currentEventType;
        private IReadOnlyList<ChoiceData> _currentChoices;
        private TurnResult _currentResult;
        private EmergencyRecoveryData _emergencyRecovery;
        private GovernanceResultRecord _governanceResult;
        private GameOverPresentationData _gameOverData;
        private bool _hasEmergencyRecoveryResult;
        private bool _emergencyRecoverySucceeded;
        private KingdomStatsSnapshot _displayedStats;
        private Vector2 _scrollPosition;
        private string _statusText;
        private bool _showChoices;
        private bool _showResult;
        private bool _isCancelled;
        private GUIStyle _bodyStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _panelStyle;

        public override IEnumerator ShowGameStart(GameRuntimeState state)
        {
            _runtimeState = state;
            _displayedStats = state.Stats.CreateSnapshot();
            _currentEvent = null;
            _currentChoices = null;
            _currentResult = null;
            _emergencyRecovery = null;
            _governanceResult = null;
            _gameOverData = null;
            _hasEmergencyRecoveryResult = false;
            _emergencyRecoverySucceeded = false;
            _showChoices = false;
            _showResult = false;
            _isCancelled = false;
            _statusText = "게임 시작";
            yield break;
        }

        public override IEnumerator ShowWeekStart(GameRuntimeState runtimeState)
        {
            _statusText = $"{runtimeState.CurrentWeek}주차 시작";
            yield break;
        }

        public override IEnumerator ShowEvent(EventData eventData)
        {
            _currentEvent = eventData;
            _emergencyRecovery = null;
            _hasEmergencyRecoveryResult = false;
            _currentEventWeek = _runtimeState.CurrentWeek;
            _currentEventSlotIndex = _runtimeState.CurrentSlotIndex;
            _currentEventType = eventData.eventType;
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
                    TryFindChoice(choices, KeyCode.Q, out ChoiceData qChoice))
                {
                    SelectChoice(qChoice, onSelected);
                    yield break;
                }

                if (keyboard.spaceKey.wasPressedThisFrame &&
                    TryFindChoice(choices, KeyCode.Space, out ChoiceData spaceChoice))
                {
                    SelectChoice(spaceChoice, onSelected);
                    yield break;
                }

                if (keyboard.pKey.wasPressedThisFrame &&
                    TryFindChoice(choices, KeyCode.P, out ChoiceData pChoice))
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

        public override IEnumerator ShowWeekSettlement(GameRuntimeState runtimeState)
        {
            _statusText = $"{runtimeState.CurrentWeek}주차 정산 완료";
            yield break;
        }

        public override IEnumerator ShowEmergencyRecovery(EmergencyRecoveryData data)
        {
            _emergencyRecovery = data;
            _statusText = $"긴급 수습 발생: {data.title} (성공 {data.successProbability:P0})";
            yield break;
        }

        public override IEnumerator ShowEmergencyRecoveryResult(EmergencyRecoveryData data,
                                                                  bool succeeded)
        {
            _hasEmergencyRecoveryResult = true;
            _emergencyRecoverySucceeded = succeeded;
            _statusText = succeeded ? data.successText : data.failureText;
            yield break;
        }

        public override IEnumerator ShowGameOver(GameOverPresentationData data)
        {
            _showChoices = false;
            _gameOverData = data;
            _statusText = $"게임 오버: {data.title}";
            yield break;
        }

        public override IEnumerator ShowClear()
        {
            _showChoices = false;
            _statusText = "게임 클리어";
            yield break;
        }

        public override IEnumerator ShowGovernanceResult(GovernanceResultRecord record)
        {
            _governanceResult = record;
            _statusText = "통치 결과";
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

            EnsureStyles();

            float scale = Mathf.Min(Screen.width / ReferenceWidth,
                                    Screen.height / ReferenceHeight);
            scale = Mathf.Max(scale, 0.01f);

            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float scaledScreenWidth = Screen.width / scale;
            float scaledScreenHeight = Screen.height / scale;

            Rect area = new(scaledScreenWidth - PanelWidth - PanelMargin,
                            PanelMargin,
                            PanelWidth,
                            scaledScreenHeight - PanelMargin * 2f);

            GUILayout.BeginArea(area, _panelStyle);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawGameState();
            DrawStats();
            DrawEvent();
            DrawChoices();
            DrawResult();
            DrawEnding();
            DrawStatus();

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.matrix = previousMatrix;
        }

        private void DrawGameState()
        {
            GUILayout.Label("[Game Flow]", _headerStyle);
            GUILayout.Label($"State: {GetCurrentStateText()}", _bodyStyle);
            GUILayout.Label($"현재 주차: {_runtimeState.CurrentWeek}주차", _bodyStyle);
            GUILayout.Label($"주차 진행: {Mathf.Min(_runtimeState.EventsCompletedThisWeek + 1, 4)} / 4", _bodyStyle);
            GUILayout.Label($"전체 진행: {_runtimeState.ProcessedEventCount} / 12", _bodyStyle);
            GUILayout.Label($"이번 주 슬롯: {FormatWeekSlots()}", _bodyStyle);
            GUILayout.Space(16f);
        }

        private void DrawStats()
        {
            GUILayout.Label("[왕국 수치]", _headerStyle);
            GUILayout.Label($"국고: {_displayedStats.Treasury}", _bodyStyle);
            GUILayout.Label($"민심: {_displayedStats.PublicSentiment}", _bodyStyle);
            GUILayout.Label($"안보: {_displayedStats.Security}", _bodyStyle);
            GUILayout.Space(16f);
        }

        private void DrawEvent()
        {
            if (_currentEvent == null)
            {
                return;
            }

            GUILayout.Label("[이벤트]", _headerStyle);
            GUILayout.Label($"위치: {_currentEventWeek}주차 {_currentEventSlotIndex + 1}번째", _bodyStyle);
            GUILayout.Label($"유형: {GetEventTypeText(_currentEventType)}", _headerStyle);
            if (_currentEvent.isConditional)
            {
                GUILayout.Label($"[조건부] 대상: {GetStatTypeText(_currentEvent.conditionalStat)}", _headerStyle);
            }
            GUILayout.Label(_currentEvent.title, _bodyStyle);
            GUILayout.Label(_currentEvent.description, _bodyStyle);
            GUILayout.Space(16f);
        }

        private void DrawChoices()
        {
            if (!_showChoices || _currentChoices == null)
            {
                return;
            }

            GUILayout.Label("[선택지]", _headerStyle);
            foreach (ChoiceData choice in _currentChoices)
            {
                if (choice == null)
                {
                    continue;
                }

                GUILayout.Label($"[{choice.inputKey}] {choice.description}", _bodyStyle);
                if (!string.IsNullOrWhiteSpace(choice.changePreview))
                {
                    GUILayout.Label($"    {choice.changePreview}", _bodyStyle);
                }
            }

            GUILayout.Space(16f);
        }

        private void DrawResult()
        {
            if (!_showResult || _currentResult == null)
            {
                return;
            }

            GUILayout.Label("[결과]", _headerStyle);
            GUILayout.Label(_currentResult.ResultText, _bodyStyle);
            if (_currentResult.HasActionResult)
            {
                GUILayout.Label($"액션 결과: {(_currentResult.ActionSucceeded ? "성공" : "실패")}", _bodyStyle);
            }
            if (_currentResult.HasRandomResult)
            {
                GUILayout.Label($"확률 결과: {(_currentResult.RandomResultSucceeded ? "성공" : "실패")}", _bodyStyle);
            }
            GUILayout.Label($"국고 {FormatModifier(_currentResult.FinalModifier.treasury)} / " +
                            $"민심 {FormatModifier(_currentResult.FinalModifier.publicSentiment)} / " +
                            $"안보 {FormatModifier(_currentResult.FinalModifier.security)}", _bodyStyle);
            GUILayout.Space(16f);
        }

        private void DrawStatus()
        {
            GUILayout.Label("[상태]", _headerStyle);
            GUILayout.Label(_statusText ?? string.Empty, _bodyStyle);
        }

        private string GetCurrentStateText()
        {
            GameFlowController controller = GetComponent<GameFlowController>();
            return controller != null ? controller.CurrentState.ToString() : "Unknown";
        }

        private void EnsureStyles()
        {
            if (_bodyStyle != null)
            {
                return;
            }

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = BodyFontSize,
                wordWrap = true,
                richText = true,
                normal =
                {
                    textColor = Color.white
                }
            };

            _headerStyle = new GUIStyle(_bodyStyle)
            {
                fontSize = HeaderFontSize,
                fontStyle = FontStyle.Bold
            };

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(24, 24, 24, 24)
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
                                          KeyCode inputKey,
                                          out ChoiceData result)
        {
            foreach (ChoiceData choice in choices)
            {
                if (choice != null && choice.inputKey == inputKey)
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
            _statusText = $"선택: {choice.inputKey} - {choice.description}";
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

        private string FormatWeekSlots()
        {
            IReadOnlyList<EEventType> slots = _runtimeState.CurrentWeekSlots;
            if (slots == null || slots.Count == 0)
            {
                return "없음";
            }

            var slotTexts = new string[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                string slotText = $"{i + 1}.{GetEventTypeText(slots[i])}";
                slotTexts[i] = i == _currentEventSlotIndex &&
                               _currentEventWeek == _runtimeState.CurrentWeek
                    ? $"<b>[{slotText}]</b>"
                    : slotText;
            }

            return string.Join("  |  ", slotTexts);
        }

        private static string GetEventTypeText(EEventType eventType)
        {
            return eventType switch
            {
                EEventType.NormalChoice => "기본 선택",
                EEventType.ActionChoice => "실행형 액션",
                EEventType.SuddenChoice => "돌발 액션",
                _ => eventType.ToString()
            };
        }

        private void DrawEnding()
        {
            if (_emergencyRecovery != null)
            {
                GUILayout.Label("[긴급 수습]", _headerStyle);
                GUILayout.Label(_emergencyRecovery.title, _headerStyle);
                GUILayout.Label(_emergencyRecovery.description, _bodyStyle);
                GUILayout.Label($"성공 확률: {_emergencyRecovery.successProbability:P0}", _bodyStyle);
                if (_hasEmergencyRecoveryResult)
                {
                    GUILayout.Label($"수습 결과: {(_emergencyRecoverySucceeded ? "성공" : "실패")}", _headerStyle);
                    GUILayout.Label(_emergencyRecoverySucceeded
                                        ? _emergencyRecovery.successText
                                        : _emergencyRecovery.failureText,
                                    _bodyStyle);
                }
                GUILayout.Space(16f);
            }

            if (_gameOverData != null)
            {
                GUILayout.Label("[게임 오버]", _headerStyle);
                GUILayout.Label(_gameOverData.title, _headerStyle);
                GUILayout.Label(_gameOverData.presentation, _bodyStyle);
                GUILayout.Label(_gameOverData.message, _bodyStyle);
                GUILayout.Space(16f);
            }

            if (_governanceResult == null)
            {
                return;
            }

            GUILayout.Label("[통치 결과]", _headerStyle);
            GUILayout.Label($"결과: {(_governanceResult.IsClear ? "클리어" : "게임 오버")}", _bodyStyle);
            GUILayout.Label($"통치 기간: {_governanceResult.ReignWeek}주차", _bodyStyle);
            GUILayout.Label($"해결한 사건: {_governanceResult.ResolvedEventCount}", _bodyStyle);
            GUILayout.Label($"최종 국고: {_governanceResult.FinalStats.Treasury}", _bodyStyle);
            GUILayout.Label($"최종 민심: {_governanceResult.FinalStats.PublicSentiment}", _bodyStyle);
            GUILayout.Label($"최종 안보: {_governanceResult.FinalStats.Security}", _bodyStyle);
            GUILayout.Label($"액션 성공/실패: {_governanceResult.ActionSuccessCount} / {_governanceResult.ActionFailureCount}", _bodyStyle);
            GUILayout.Label($"긴급 수습 발생: {_governanceResult.EmergencyOccurred}", _bodyStyle);
            GUILayout.Label($"긴급 수습 성공: {_governanceResult.EmergencySucceeded}", _bodyStyle);
            GUILayout.Space(16f);
        }

        private static string GetStatTypeText(EKingdomStatType statType)
        {
            return statType switch
            {
                EKingdomStatType.Treasury => "국고",
                EKingdomStatType.PublicSentiment => "민심",
                EKingdomStatType.Security => "안보",
                _ => statType.ToString()
            };
        }
    }
}
