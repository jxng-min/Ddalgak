using DG.Tweening;
using JxModule;

namespace Ddalgak
{
    public class TitleLabelView : LabelView
    {
        private void Awake()
        {
            var sequence = DOTween.Sequence();

            sequence.Append(
                CanvasGroup.DOFade(1f, 0.5f)
            );

            sequence.Append(
                CanvasGroup.DOFade(0f, 0.5f)
            );
            
            sequence.SetLoops(-1, LoopType.Yoyo);
        }
    }
}