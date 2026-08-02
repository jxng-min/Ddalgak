using System.Collections;
using DG.Tweening;
using JxModule;
using UnityEngine;

namespace Ddalgak
{
    public class StatView : ViewBase
    {
        [BigHeader("Effect")]
        [SerializeField] private StatEffect statEffect;

        private Vector2 _originAnchoredPosition;

        private void Awake()
        {
            _originAnchoredPosition = RectTransform.anchoredPosition;
        }

        public IEnumerator InitStat()
        {
            SoundManager.Instance.PlaySfx("SFX_StatusAppear");
            yield return statEffect.PlayInitStatEffect(RectTransform, _originAnchoredPosition).WaitForCompletion();
        }
    }
}