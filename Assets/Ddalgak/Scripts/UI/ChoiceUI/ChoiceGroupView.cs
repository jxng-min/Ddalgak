using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class ChoiceGroupView : MonoBehaviour
    {
        private static readonly Key[] SlotKeys = { Key.P, Key.Q, Key.Space };

        [Header("Slots (P / Q / Space)")]
        [SerializeField] private ChoiceButtonView leftView;
        [SerializeField] private ChoiceButtonView rightView;
        [SerializeField] private ChoiceButtonView centerView;

        [Header("Timing")]
        [SerializeField] private float showStaggerInterval = 0.1f;

        [Header("Result Stamp")]
        [SerializeField] private StampEffect stampEffect;
        [SerializeField] private ButtonActionView buttonActionView;

        private readonly List<ChoiceButtonView> _activeViews = new();
        private bool _inputLocked;

        public IEnumerator ShowInitialButtons()
        {
            ChoiceButtonView[] views = { leftView, rightView, centerView };
            Sequence sequence = DOTween.Sequence();

            for (var i = 0; i < views.Length; i++)
            {
                Tween tween = views[i].PlayInitialDrop();
                if (tween != null)
                {
                    sequence.Insert(i * showStaggerInterval, tween);
                }
            }

            yield return sequence.WaitForCompletion();
        }

        public IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices, Action<ChoiceData> onSelected)
        {
            _inputLocked = false;
            BindActiveViews(choices);

            if (_activeViews.Count == 0)
            {
                yield break;
            }

            yield return PlayShowSequence();

            ChoiceButtonView selectedView = null;
            yield return WaitForInput(view => selectedView = view);

            if (selectedView == null)
            {
                yield break;
            }

            yield return PlaySelectSequence(selectedView);

            onSelected?.Invoke(selectedView.Data);
        }

        public IEnumerator HideChoices()
        {
            foreach (ChoiceButtonView view in _activeViews)
            {
                view.ResetSelectionVisual();
                view.HideDescription();
            }

            yield break;
        }

        public void Cancel()
        {
            _inputLocked = true;
            buttonActionView?.Hide();

            ChoiceButtonView[] views = { leftView, rightView, centerView };
            foreach (ChoiceButtonView view in views)
            {
                view.KillTweens();
            }

            stampEffect.Kill();
        }

        public IEnumerator ShowResultStamp(TurnResult result)
        {
            yield return stampEffect.Play(IsSuccess(result)).WaitForCompletion();
        }

        public void ResetResultStamp()
        {
            stampEffect.Reset();
        }

        private static bool IsSuccess(TurnResult result)
        {
            if (result.HasActionResult)
            {
                return result.ActionSucceeded;
            }

            if (result.HasRandomResult)
            {
                return result.RandomResultSucceeded;
            }

            return true;
        }

        private void BindActiveViews(IReadOnlyList<ChoiceData> choices)
        {
            _activeViews.Clear();

            Dictionary<Key, ChoiceData> choiceByKey = new();
            if (choices != null)
            {
                foreach (ChoiceData choice in choices)
                {
                    if (choice == null)
                    {
                        continue;
                    }

                    if (!choiceByKey.TryAdd(choice.inputKey, choice))
                    {
                        Debug.LogWarning($"[ChoiceGroupView] Duplicate input key in choices: {choice.inputKey}");
                    }
                }
            }

            foreach (Key key in SlotKeys)
            {
                ChoiceButtonView view = GetViewForKey(key);
                if (view == null)
                {
                    continue;
                }

                if (!choiceByKey.TryGetValue(key, out ChoiceData data))
                {
                    Debug.LogWarning($"[ChoiceGroupView] No choice data for input key: {key}");
                    continue;
                }

                view.Bind(data);
                _activeViews.Add(view);
            }
        }

        private ChoiceButtonView GetViewForKey(Key key)
        {
            return key switch
            {
                Key.P => leftView,
                Key.Q => rightView,
                Key.Space => centerView,
                _ => null
            };
        }

        private IEnumerator PlayShowSequence()
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < _activeViews.Count; i++)
            {
                Tween tween = _activeViews[i].PlayShow();
                if (tween != null)
                {
                    sequence.Insert(i * showStaggerInterval, tween);
                }
            }

            yield return sequence.WaitForCompletion();
        }

        private IEnumerator WaitForInput(Action<ChoiceButtonView> onSelected)
        {
            while (!_inputLocked)
            {
                foreach (ChoiceButtonView view in _activeViews)
                {
                    if (Keyboard.current[view.InputKey].wasPressedThisFrame)
                    {
                        _inputLocked = true;
                        SoundManager.Instance.PlaySfx("SFX_ButtonNormalClick");
                        buttonActionView?.Show(EButtonActionType.None, view.InputKey);

                        yield return new WaitUntil(() => Keyboard.current == null ||
                                                         !Keyboard.current[view.InputKey].isPressed);

                        buttonActionView?.Hide();
                        onSelected(view);
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private IEnumerator PlaySelectSequence(ChoiceButtonView selectedView)
        {

            Tween selectTween = null;
            foreach (ChoiceButtonView view in _activeViews)
            {
                if (view == selectedView)
                {
                    selectTween = view.PlaySelect();
                }
            }

            if (selectTween != null)
            {
                yield return selectTween.WaitForCompletion();
            }
        }
    }
}
