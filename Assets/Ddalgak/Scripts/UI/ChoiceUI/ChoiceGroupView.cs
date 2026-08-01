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

        private readonly List<ChoiceButtonView> _activeViews = new();
        private bool _inputLocked;

        public IEnumerator ShowChoices(IReadOnlyList<ChoiceData> choices, Action<ChoiceData> onSelected)
        {
            _inputLocked = false;
            stampEffect.Reset();
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
            }

            yield break;
        }

        public void Cancel()
        {
            _inputLocked = true;

            foreach (ChoiceButtonView view in _activeViews)
            {
                view.KillTweens();
            }

            stampEffect.Kill();
        }

        public Tween ShowResultStamp(TurnResult result)
        {
            return stampEffect.Play(IsSuccess(result));
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
            List<Tween> tweens = new();
            for (int i = 0; i < _activeViews.Count; i++)
            {
                if (i > 0)
                {
                    yield return new WaitForSeconds(showStaggerInterval);
                }

                tweens.Add(_activeViews[i].PlayShow());
            }

            foreach (Tween tween in tweens)
            {
                yield return tween.WaitForCompletion();
            }
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
                else
                {
                    view.PlayDim();
                }
            }

            if (selectTween != null)
            {
                yield return selectTween.WaitForCompletion();
            }
        }
    }
}