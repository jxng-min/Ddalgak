using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    [ManagedEffect("Progress", "Background", 0)]
    public class BackgroundEffect : MonoBehaviour
    {
        [BigHeader("Settings")]
        [Header("Fade In")]
        [SerializeField] private float fadeInDuration;
        [SerializeField] private Ease fadeInEase;

        public Tween PlayDrawKingdomEffect(Image kingdomImage)
        {
            return kingdomImage.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
        }
    }
}