using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ChoiceGroupView : MonoBehaviour
    {
        private static readonly KeyCode[] SlotKeys = { KeyCode.P, KeyCode.Q, KeyCode.Space };

        [Header("Slots (P / Q / Space)")]
        [SerializeField] private ChoiceButtonView leftView;
        [SerializeField] private ChoiceButtonView rightView;
        [SerializeField] private ChoiceButtonView centerView;

        [Header("Timing")]
        [SerializeField] private float showStaggerInterval = 0.1f;

        private readonly List<ChoiceButtonView> _activeViews = new();
        private bool _inputLocked;

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
        }

        private void BindActiveViews(IReadOnlyList<ChoiceData> choices)
        {
            _activeViews.Clear();

            Dictionary<KeyCode, ChoiceData> choiceByKey = new();
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

            foreach (KeyCode key in SlotKeys)
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

        private ChoiceButtonView GetViewForKey(KeyCode key)
        {
            return key switch
            {
                KeyCode.P => leftView,
                KeyCode.Q => rightView,
                KeyCode.Space => centerView,
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
                    if (Input.GetKeyDown(view.InputKey))
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