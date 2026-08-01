using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ChoiceButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private ChoiceShowEffect showEffect;
        [SerializeField] private ChoiceSelectEffect selectEffect;

        public ChoiceData Data { get; private set; }
        public KeyCode InputKey => Data?.inputKey ?? KeyCode.None;

        public void Bind(ChoiceData data)
        {
            Data = data;
            descriptionText.text = data?.description ?? string.Empty;
            ResetSelectionVisual();
        }

        public void ResetSelectionVisual()
        {
            selectEffect.ResetVisual();
        }

        public Tween PlayShow()
        {
            return showEffect.Play();
        }

        public Tween PlayDim()
        {
            return selectEffect.PlayDim();
        }

        public Sequence PlaySelect()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Join(selectEffect.PlaySelect());
            return sequence;
        }

        public void KillTweens()
        {
            showEffect.Kill();
            selectEffect.Kill();
        }
    }
}