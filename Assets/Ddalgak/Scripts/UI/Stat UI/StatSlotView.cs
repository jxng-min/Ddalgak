using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    public class StatSlotView : ViewBase
    {
        [BigHeader("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image sliderImage;

        [Space(30f)]
        [BigHeader("Effect")]
        [SerializeField] private StatSlotEffect slotEffect;

        private Vector2 _originAnchoredPosition;
        private Tween _valueChangeTween;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
        }
        
        public IEnumerator IncreaseRate(float rate)
        {
            _valueChangeTween?.Kill();
            _valueChangeTween = slotEffect.PlayIncreaseValueEffect(RectTransform, _originAnchoredPosition, sliderImage, rate);
            yield return _valueChangeTween.WaitForCompletion();
        }

        public IEnumerator DecreaseRate(float rate)
        {
            _valueChangeTween?.Kill();
            _valueChangeTween = slotEffect.PlayDecreaseValueEffect(RectTransform, sliderImage, rate);
            yield return _valueChangeTween.WaitForCompletion();
        }

        public void SetRate(float rate)
        {
            sliderImage.fillAmount = rate;
        }

        public void CancelUpdateRate()
        {
            _valueChangeTween?.Kill();
            _valueChangeTween = null;
        }
    }
}