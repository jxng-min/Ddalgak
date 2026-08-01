using System.Collections;
using KoreanTyper;
using TMPro;
using UnityEngine;

namespace Ddalgak
{
    public class KoreanTyper : MonoBehaviour
    {
        public IEnumerator TypeByInterval(System.Action<string> onTextChanged,
                                          string text,
                                          float typingInterval,
                                          float startDelay = 0f,
                                          System.Func<bool> shouldCancel = null)
        {
            text ??= string.Empty;
            onTextChanged?.Invoke(string.Empty);

            if (startDelay > 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }

            var typingLength = text.GetTypingLength();
            for (var i = 0; i <= typingLength; i++)
            {
                if (shouldCancel?.Invoke() == true)
                {
                    yield break;
                }

                onTextChanged?.Invoke(text.Typing(i));
                yield return new WaitForSeconds(typingInterval);
            }
        }

        public IEnumerator TypeByInterval(TMP_Text label, string text, float typingInterval, float startDelay = 0f)
        {
            label.text = string.Empty;

            if (startDelay >= 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }
            
            var strTypingLength = text.GetTypingLength();
            
            for (var i = 0 ; i <= strTypingLength ; i++) 
            {
                label.text = text.Typing(i);
                yield return new WaitForSeconds(typingInterval);
            }
        }

        public IEnumerator TypeByDuration(TMP_Text label, string text, float duration, float startDelay = 0f)
        {
            label.text = string.Empty;

            if (startDelay >= 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }
            
            var strTypingLength = text.GetTypingLength();
            var typingInterval = duration / strTypingLength;

            for (var i = 0; i <= strTypingLength; i++)
            {
                label.text = text.Typing(i);
                yield return new WaitForSeconds(typingInterval);
            }
        }
    }
}
