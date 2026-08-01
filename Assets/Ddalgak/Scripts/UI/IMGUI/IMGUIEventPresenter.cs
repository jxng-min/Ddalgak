using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class IMGUIEventPresenter : EventPresenterBase
    {
        [Header("Typing")]
        [SerializeField] private KoreanTyper koreanTyper;
        [SerializeField, Min(0f)] private float typingInterval = 0.035f;
        [SerializeField, Min(0f)] private float typingStartDelay = 0.2f;

        [Header("Layout")]
        [SerializeField] private GUISkin guiSkin;
        [SerializeField, Min(320f)] private float panelWidth = 720f;
        [SerializeField, Min(120f)] private float imageHeight = 320f;
        [SerializeField, Min(40f)] private float descriptionHeight = 120f;

        [Header("Typography")]
        [SerializeField, Min(12)] private int titleFontSize = 40;
        [SerializeField, Min(12)] private int descriptionFontSize = 26;
        [SerializeField, Min(12)] private int buttonFontSize = 24;
        [SerializeField, Min(12)] private int resultFontSize = 26;
        [SerializeField, Min(32f)] private float buttonHeight = 58f;

        private readonly List<ChoiceData> _choices = new();
        private EventData _eventData;
        private Action<ChoiceData> _onSelected;
        private string _description = string.Empty;
        private string _resultText = string.Empty;
        private bool _isEventVisible;
        private bool _isChoicesVisible;
        private bool _isResultVisible;
        private bool _isWaitingForNextTurn;
        private bool _nextTurnRequested;
        private bool _cancelRequested;
        private bool _choiceSelected;

        public override IEnumerator ShowEvent(EventData eventData)
        {
            _cancelRequested = false;
            _eventData = eventData;
            _description = string.Empty;
            _resultText = string.Empty;
            _isResultVisible = false;
            _isEventVisible = eventData != null;

            if (eventData == null)
            {
                yield break;
            }

            if (koreanTyper == null)
            {
                Debug.LogWarning("KoreanTyper is not assigned to IMGUIEventPresenter.", this);
                _description = eventData.description ?? string.Empty;
                yield break;
            }

            yield return koreanTyper.TypeByInterval(
                value => _description = value,
                eventData.description,
                typingInterval,
                typingStartDelay,
                () => _cancelRequested);
        }

        public override IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices,
                                                Action<ChoiceData> onSelected)
        {
            _choices.Clear();
            if (choices != null)
            {
                foreach (var choice in choices)
                {
                    if (choice != null)
                    {
                        _choices.Add(choice);
                    }
                }
            }

            _onSelected = onSelected;
            _choiceSelected = false;
            _isChoicesVisible = _choices.Count > 0;
            yield break;
        }

        public override IEnumerator HideChoices()
        {
            _isChoicesVisible = false;
            _choices.Clear();
            _onSelected = null;
            yield break;
        }

        public override IEnumerator ShowResult(TurnResult result)
        {
            _resultText = result?.ResultText ?? string.Empty;
            _isResultVisible = !string.IsNullOrWhiteSpace(_resultText);
            yield break;
        }

        public override IEnumerator WaitForNextTurnInput()
        {
            _nextTurnRequested = false;
            _isWaitingForNextTurn = true;
            yield return new WaitUntil(() => _nextTurnRequested || _cancelRequested);
            _isWaitingForNextTurn = false;
        }

        public override IEnumerator HideEvent()
        {
            _isEventVisible = false;
            _isChoicesVisible = false;
            _eventData = null;
            _description = string.Empty;
            _choices.Clear();
            _onSelected = null;
            yield break;
        }

        public override void Cancel()
        {
            _cancelRequested = true;
            _isEventVisible = false;
            _isChoicesVisible = false;
            _isResultVisible = false;
            _isWaitingForNextTurn = false;
            _nextTurnRequested = true;
            _eventData = null;
            _choices.Clear();
            _onSelected = null;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (_isChoicesVisible && !_choiceSelected)
            {
                foreach (var choice in _choices)
                {
                    if (keyboard[choice.inputKey].wasPressedThisFrame)
                    {
                        SelectChoice(choice);
                        break;
                    }
                }
            }

            if (_isWaitingForNextTurn &&
                (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame ||
                 keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                _nextTurnRequested = true;
            }
        }

        private void OnGUI()
        {
            if (!_isEventVisible && !_isResultVisible && !_isWaitingForNextTurn)
            {
                return;
            }

            if (guiSkin != null)
            {
                GUI.skin = guiSkin;
            }

            var width = Mathf.Min(panelWidth, Mathf.Max(100f, Screen.width - 32f));
            var height = Mathf.Max(100f, Screen.height - 32f);
            var panelRect = new Rect((Screen.width - width) * 0.5f, 16f, width, height);

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.BeginVertical();

            var descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = descriptionFontSize,
                wordWrap = true,
                richText = true
            };
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = buttonFontSize,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            var resultStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = resultFontSize,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            if (_isEventVisible && _eventData != null)
            {
                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = titleFontSize,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
                GUILayout.Label(_eventData.title ?? string.Empty, titleStyle);
                DrawEventImage(_eventData.eventImage);
                GUILayout.Label(_description, descriptionStyle, GUILayout.MinHeight(descriptionHeight));
                DrawChoices(buttonStyle);
            }

            if (_isResultVisible)
            {
                GUILayout.Space(12f);
                GUILayout.Label(_resultText, resultStyle, GUILayout.MinHeight(80f));
            }

            if (_isWaitingForNextTurn &&
                GUILayout.Button("다음", buttonStyle, GUILayout.Height(buttonHeight)))
            {
                _nextTurnRequested = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawChoices(GUIStyle buttonStyle)
        {
            if (!_isChoicesVisible || _choiceSelected)
            {
                return;
            }

            GUILayout.Space(12f);
            foreach (var choice in _choices)
            {
                var label = $"[{choice.inputKey}] {choice.description}";
                if (GUILayout.Button(label, buttonStyle, GUILayout.Height(buttonHeight)))
                {
                    SelectChoice(choice);
                }
            }
        }

        private void SelectChoice(ChoiceData choice)
        {
            if (_choiceSelected || choice == null)
            {
                return;
            }

            _choiceSelected = true;
            _isChoicesVisible = false;
            _onSelected?.Invoke(choice);
        }

        private void DrawEventImage(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            var layoutRect = GUILayoutUtility.GetRect(1f, imageHeight, GUILayout.ExpandWidth(true));
            var spriteRect = sprite.textureRect;
            var aspect = spriteRect.width / spriteRect.height;
            var drawRect = FitRect(layoutRect, aspect);
            var texture = sprite.texture;
            var uvRect = new Rect(spriteRect.x / texture.width,
                                  spriteRect.y / texture.height,
                                  spriteRect.width / texture.width,
                                  spriteRect.height / texture.height);
            GUI.DrawTextureWithTexCoords(drawRect, texture, uvRect, true);
        }

        private static Rect FitRect(Rect bounds, float aspect)
        {
            var width = bounds.width;
            var height = width / aspect;
            if (height > bounds.height)
            {
                height = bounds.height;
                width = height * aspect;
            }

            return new Rect(bounds.x + (bounds.width - width) * 0.5f,
                            bounds.y + (bounds.height - height) * 0.5f,
                            width,
                            height);
        }
    }
}
