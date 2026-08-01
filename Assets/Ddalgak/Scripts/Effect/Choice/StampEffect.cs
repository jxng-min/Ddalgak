using DG.Tweening;
using UnityEngine;

namespace Ddalgak
{
    [ManagedEffect("Choice", "결과 도장 효과", 2)]
    public sealed class StampEffect : MonoBehaviour
    {
        [Header("Stamps")]
        [SerializeField] private GameObject successStamp;
        [SerializeField] private GameObject failureStamp;

        [Header("Slam")]
        [SerializeField] private float startScale = 1.6f;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutBack;

        public Tween Play(bool isSuccess)
        {
            GameObject stamp = isSuccess ? successStamp : failureStamp;
            GameObject otherStamp = isSuccess ? failureStamp : successStamp;
            otherStamp.SetActive(false);

            RectTransform rectTransform = (RectTransform)stamp.transform;
            DOTween.Kill(rectTransform);

            stamp.SetActive(true);
            rectTransform.localScale = Vector3.one * startScale;

            return rectTransform.DOScale(1f, duration).SetEase(ease);
        }

        public void Kill()
        {
            DOTween.Kill(successStamp.transform);
            DOTween.Kill(failureStamp.transform);
        }

        public void Reset()
        {
            Kill();
            successStamp.SetActive(false);
            failureStamp.SetActive(false);
        }
    }
}
