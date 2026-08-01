using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    [ManagedEffect("Stat UI", "Stat Slot", 0)]
    public class StatSlotEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Value Change")]
        [SerializeField] private float valueChangeDuration = 0.2f;
        
        [Header("Value Increase")]
        [SerializeField, Range(0.1f, 0.5f)] private float increaseDuration = 0.2f;
        [SerializeField] private float increaseYOffset = 10f;
        [SerializeField] private Ease increaseEase = Ease.Unset;
        
        [Header("Value Decrease")]
        [SerializeField, Range(0.1f, 0.5f)] private float decreaseDuration = 0.2f;
        [SerializeField, Range(0.1f, 0.5f)] private float punchDuration = 0.2f; 
        [SerializeField] private Vector3 punchPosition = new Vector3(5f, 0f, 0f);
        [SerializeField] private Ease decreaseEase = Ease.Unset;

        public Tween PlayIncreaseValueEffect(RectTransform slotRect, 
                                             Vector2 originAnchoredPosition, 
                                             Image sliderImage, 
                                             float valueRate)
        {
            var sequence = DOTween.Sequence();

            sequence.Join(
                slotRect.DOAnchorPosY(originAnchoredPosition.y + increaseYOffset, increaseDuration)
                        .SetEase(increaseEase)
            );

            sequence.Append(
                sliderImage.DOFillAmount(valueRate, valueChangeDuration)
            );

            sequence.Append(
                slotRect.DOAnchorPosY(originAnchoredPosition.y, increaseDuration)
                    .SetEase(increaseEase)
            );

            return sequence;
        }

        public Tween PlayDecreaseValueEffect(RectTransform slotRect, Image sliderImage, float valueRate)
        {
            var sequence = DOTween.Sequence();
            
            sequence.Join(
                slotRect.DOPunchPosition(punchPosition, punchDuration)
                    .SetEase(decreaseEase)
            );

            sequence.Join(
                sliderImage.DOFillAmount(valueRate, decreaseDuration)
            );

            return sequence;
        }
    }
}