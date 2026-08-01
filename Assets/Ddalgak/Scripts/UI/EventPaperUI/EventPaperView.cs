using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventPaperView : MonoBehaviour
    {
        [SerializeField] private DropInEffect enterEffect;
        [SerializeField] private TMP_Text descriptionText;

        public bool IsAnimating { get; private set; }

        public IEnumerator ShowPaper()
        {
            IsAnimating = true;

            yield return enterEffect.Play().WaitForCompletion();

            IsAnimating = false;
        }
        


        public void Cancel()
        {
            IsAnimating = false;
            enterEffect.Kill();
        }
    }
}