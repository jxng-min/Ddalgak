using DG.Tweening;
using JxModule;
using UnityEngine;
using UnityEngine.UI;

namespace Ddalgak
{
    public class BackgroundView : MonoBehaviour
    {
        [BigHeader("UI")]
        [SerializeField] private Image kingdomImage;
        
        [Space(30f)]
        [SerializeField] private BackgroundEffect backgroundEffect;

        public YieldInstruction DrawKingdom()
        {
            return backgroundEffect.PlayDrawKingdomEffect(kingdomImage).WaitForCompletion();
        }
    }
}