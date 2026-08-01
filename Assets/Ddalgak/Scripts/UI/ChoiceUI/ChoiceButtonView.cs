using DG.Tweening;
using JxModule;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ddalgak
{
    public sealed class ChoiceButtonView : ViewBase
    {
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private ChoiceButtonDropEffect initialDropEffect;
        [SerializeField] private ChoiceShowEffect showEffect;
        [SerializeField] private ChoiceSelectEffect selectEffect;
        [SerializeField] private GameObject textImage;

        [Header("Typing")]
        [SerializeField] private KoreanTyper koreanTyper;
        [SerializeField, Min(0f)] private float typingInterval = 0.035f;
        [SerializeField, Min(0f)] private float typingStartDelay = 0f;

        private Coroutine _typingCoroutine;

        public ChoiceData Data { get; private set; }
        public Key InputKey => Data?.inputKey ?? Key.None;

        private void Awake()
        {
            initialDropEffect.Prepare(RectTransform);
        }

        public void Bind(ChoiceData data)
        {
            Data = data;
            ShowDescription(data?.description);
            ResetSelectionVisual();
        }

        public void ResetSelectionVisual()
        {
            selectEffect.ResetVisual();
        }

        public Tween PlayInitialDrop()
        {
            return initialDropEffect.Play(RectTransform);
        }

        public Tween PlayShow()
        {
            return showEffect.Play();
        }

        public Sequence PlaySelect()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Join(selectEffect.PlaySelect());
            return sequence;
        }

        public void ShowDescription(string text)
        {
            textImage.SetActive(true);
            SetDescriptionText(text);
        }

        public void HideDescription()
        {
            StopTyping();
            textImage.SetActive(false);
        }

        public void SetDescriptionText(string text)
        {
            StopTyping();

            text ??= string.Empty;
            if (koreanTyper == null)
            {
                descriptionText.text = text;
                return;
            }

            _typingCoroutine = StartCoroutine(koreanTyper.TypeByInterval(descriptionText, text, typingInterval, typingStartDelay));
        }

        public void KillTweens()
        {
            StopTyping();
            initialDropEffect.Kill();
            showEffect.Kill();
            selectEffect.Kill();
        }

        private void StopTyping()
        {
            if (_typingCoroutine == null)
            {
                return;
            }

            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
    }
}
