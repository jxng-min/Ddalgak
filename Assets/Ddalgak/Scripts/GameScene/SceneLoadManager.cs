using DG.Tweening;
using JxModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ddalgak
{
    public class SceneLoadManager : GlobalSingleton<SceneLoadManager>
    {
        [Header("Fade")]
        [SerializeField]
        private CanvasGroup fadeCanvasGroup;
        [SerializeField]
        private float fadeDuration;

        public bool IsLoading { get; private set; }
        public float LoadProgress { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = CreateFadeCanvasGroup();
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }


        private CanvasGroup CreateFadeCanvasGroup()
        {
            GameObject canvasObject = new("SceneFadeCanvas");
            canvasObject.transform.SetParent(transform);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject imageObject = new("FadeImage");
            imageObject.transform.SetParent(canvasObject.transform, false);

            Image image = imageObject.AddComponent<Image>();
            image.color = Color.black;

            RectTransform rectTransform = image.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return canvasObject.AddComponent<CanvasGroup>();
        }

        public void LoadScene(ESceneType sceneType, Action onCompleted = null)
        {
            if (IsLoading)
            {
                return;
            }

            StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync((int)sceneType), onCompleted));
        }

        public void ReloadCurrentScene(Action onCompleted = null)
        {
            LoadScene((ESceneType)SceneManager.GetActiveScene().buildIndex, onCompleted);
        }

        private IEnumerator LoadSceneRoutine(Func<AsyncOperation> beginLoad, Action onCompleted)
        {
            IsLoading = true;
            LoadProgress = 0f;
            fadeCanvasGroup.blocksRaycasts = true;

            yield return fadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();

            AsyncOperation operation = beginLoad();
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                LoadProgress = operation.progress / 0.9f;
                yield return null;
            }

            LoadProgress = 1f;
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            yield return fadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).WaitForCompletion();
            fadeCanvasGroup.blocksRaycasts = false;

            IsLoading = false;
            onCompleted?.Invoke();
        }
    }
}
