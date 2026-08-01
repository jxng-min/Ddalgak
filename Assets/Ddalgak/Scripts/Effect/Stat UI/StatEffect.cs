using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Stat UI", "Stat", 1)]
    public class StatEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Initialize")]
        [SerializeField] private float initXOffset;
        [SerializeField] private float initDuration;
        [SerializeField] private Ease initEase;

        public Tween PlayInitStatEffect(RectTransform statRect, Vector2 originAnchoredPosition)
        {
            return statRect.DOAnchorPosX(originAnchoredPosition.x + initXOffset, initDuration)
                           .SetEase(initEase);
        }
    }
}