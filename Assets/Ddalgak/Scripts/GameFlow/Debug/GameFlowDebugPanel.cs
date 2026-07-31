using UnityEngine;

namespace Ddalgak
{
    public sealed class GameFlowDebugPanel : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float PanelWidth = 560f;

        [SerializeField] private GameFlowController controller;
        [SerializeField] private DebugActionSequenceRunner actionRunner;

        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _panelStyle;

        private void Reset()
        {
            controller = GetComponent<GameFlowController>();
            actionRunner = GetComponent<DebugActionSequenceRunner>();
        }

        private void Awake()
        {
            controller ??= GetComponent<GameFlowController>();
            actionRunner ??= GetComponent<DebugActionSequenceRunner>();
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (controller == null)
            {
                return;
            }

            EnsureStyles();

            float scale = Mathf.Max(0.01f,
                Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight));
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUILayout.BeginArea(new Rect(30f, 30f, PanelWidth, 1020f), _panelStyle);
            GUILayout.Label("[Game Flow Test]", _headerStyle);

            DrawRuntimeState();
            DrawStatPresets();
            DrawProbabilityControls();
            DrawActionControls();
            DrawFlowControls();

            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
#endif
        }

        private void DrawRuntimeState()
        {
            GameRuntimeState state = controller.RuntimeState;
            GUILayout.Label($"Flow: {controller.CurrentState}", _labelStyle);
            GUILayout.Label($"국고 {state.Stats.Treasury} / 민심 {state.Stats.PublicSentiment} / 안보 {state.Stats.Security}", _labelStyle);
            GUILayout.Label($"긴급 수습 사용: {state.HasUsedEmergencyRecovery}", _labelStyle);
            GUILayout.Label($"주차 조건부 등장: {state.HasConditionalEventThisWeek}", _labelStyle);
            GUILayout.Label($"동일 유형 연속: {state.ConsecutiveSameEventTypeCount}", _labelStyle);
            GUILayout.Label($"액션 성공/실패: {state.ActionSuccessCount}/{state.ActionFailureCount}", _labelStyle);
            GUILayout.Space(10f);
        }

        private void DrawStatPresets()
        {
            GUILayout.Label("[수치 프리셋]", _headerStyle);
            DrawPresetButton("정상 50 / 50 / 50", 50, 50, 50);
            DrawPresetButton("국고 조건부 20 / 50 / 50", 20, 50, 50);
            DrawPresetButton("민심 조건부 50 / 20 / 50", 50, 20, 50);
            DrawPresetButton("안보 조건부 50 / 50 / 20", 50, 50, 20);
            DrawPresetButton("국고 붕괴 0 / 80 / 50", 0, 80, 50);
            DrawPresetButton("민심 붕괴 80 / 0 / 50", 80, 0, 50);
            DrawPresetButton("안보 붕괴 80 / 50 / 0", 80, 50, 0);
            DrawPresetButton("복수 붕괴 0 / 0 / 100", 0, 0, 100);
            GUILayout.Space(10f);
        }

        private void DrawProbabilityControls()
        {
            GUILayout.Label("[확률 결과]", _headerStyle);
            GUILayout.BeginHorizontal();
            DrawProbabilityButton("랜덤", EDebugOutcomeMode.Default);
            DrawProbabilityButton("성공", EDebugOutcomeMode.ForceSuccess);
            DrawProbabilityButton("실패", EDebugOutcomeMode.ForceFailure);
            GUILayout.EndHorizontal();
            GUILayout.Label($"현재: {controller.DebugProbabilityMode}", _labelStyle);

            bool forceConditional = GUILayout.Toggle(controller.DebugForceConditionalEvent,
                                                      " 조건부 이벤트 강제 추첨",
                                                      _labelStyle);
            controller.DebugForceConditionalEvent = forceConditional;
            GUILayout.Space(10f);
        }

        private void DrawActionControls()
        {
            if (actionRunner == null)
            {
                return;
            }

            GUILayout.Label("[액션 결과]", _headerStyle);
            GUILayout.BeginHorizontal();
            DrawActionButton("기본 성공", EDebugOutcomeMode.Default);
            DrawActionButton("성공", EDebugOutcomeMode.ForceSuccess);
            DrawActionButton("실패", EDebugOutcomeMode.ForceFailure);
            GUILayout.EndHorizontal();
            GUILayout.Label($"현재: {actionRunner.OutcomeMode}", _labelStyle);
            GUILayout.Space(10f);
        }

        private void DrawFlowControls()
        {
            if (GUILayout.Button("게임 재시작", _buttonStyle))
            {
                controller.RestartGame();
            }
        }

        private void DrawPresetButton(string label, int treasury, int sentiment, int security)
        {
            if (GUILayout.Button(label, _buttonStyle))
            {
                controller.DebugSetStats(treasury, sentiment, security);
            }
        }

        private void DrawProbabilityButton(string label, EDebugOutcomeMode mode)
        {
            if (GUILayout.Button(label, _buttonStyle))
            {
                controller.DebugProbabilityMode = mode;
            }
        }

        private void DrawActionButton(string label, EDebugOutcomeMode mode)
        {
            if (GUILayout.Button(label, _buttonStyle))
            {
                actionRunner.OutcomeMode = mode;
            }
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            _headerStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 38f
            };
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(20, 20, 20, 20)
            };
        }
    }
}
