using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ChoiceButtonView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private ChoiceShowEffect showEffect;
        [SerializeField] private ChoiceSelectEffect selectEffect;
        [SerializeField] private ChoiceStampEffect stampEffect;

        public ChoiceData Data { get; private set; }
        public KeyCode InputKey => Data?.inputKey ?? KeyCode.None;

        public void Bind(ChoiceData data)
        {
            Data = data;
            descriptionText.text = data?.description ?? string.Empty;
            selectEffect.ResetVisual();
            canvasGroup.alpha = 0f;
        }

        public Tween PlayShow()
        {
            return showEffect.Play();
        }

        public Tween PlayHide()
        {
            return showEffect.PlayHide();
        }

        public Tween PlayDim()
        {
            return selectEffect.PlayDim();
        }

        public Sequence PlaySelect()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Join(selectEffect.PlaySelect());
            sequence.Join(stampEffect.Play());
            return sequence;
        }

        public void KillTweens()
        {
            showEffect.Kill();
            selectEffect.Kill();
            stampEffect.Kill();
        }
    }
}
