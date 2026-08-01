using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public sealed class ChoiceButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private DropInEffect dropEffect;
        [SerializeField] private ChoiceShowEffect showEffect;
        [SerializeField] private ChoiceSelectEffect selectEffect;
        [SerializeField] private GameObject textImage;

        public ChoiceData Data { get; private set; }
        public KeyCode InputKey => Data?.inputKey ?? KeyCode.None;

        public void Bind(ChoiceData data)
        {
            Data = data;
            SetDescriptionText(data?.description);
            ResetSelectionVisual();
        }

        public void ResetSelectionVisual()
        {
            selectEffect.ResetVisual();
        }

        public Tween PlayShow()
        {
            dropEffect.Play();
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

        public void ShowDescription(string text)
        {
            textImage.SetActive(true);
            descriptionText.text = text;
        }

        public void HideDescription()
        {
            textImage.SetActive(false);
        }

        public void SetDescriptionText(string text)
        {
            descriptionText.text = text ?? string.Empty;
        }

        public void KillTweens()
        {
            showEffect.Kill();
            selectEffect.Kill();
        }
    }
}