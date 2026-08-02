using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ddalgak
{
    public sealed class ButtonActionView : MonoBehaviour
    {
        [SerializeField] private Image pActionImage;
        [SerializeField] private Image qActionImage;
        [SerializeField] private Image spaceActionImage;

        [Header("Key Images")]
        [SerializeField] private Image pKeyImage;
        [SerializeField] private Image qKeyImage;
        [SerializeField] private Image spaceKeyImage;

        [Header("Push Sprites")]
        [SerializeField] private Sprite pPushSprite;
        [SerializeField] private Sprite qPushSprite;
        [SerializeField] private Sprite spacePushSprite;

        [Header("Hold Material")]
        [SerializeField] private Material holdMaterial;

        [Header("Transition Textures")]
        [SerializeField] private Texture2D pTransitionTexture;
        [SerializeField] private Texture2D qTransitionTexture;
        [SerializeField] private Texture2D spaceTransitionTexture;

        [SerializeField] private Sprite[] singlePressFrames;
        [SerializeField] private Sprite[] pressFrames;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float frameInterval = 0.05f;

        private Image _activeImage;
        private Image _activeKeyImage;
        private Coroutine _animationCoroutine;
        private Key _activeKey = Key.None;
        private Sprite _pushSprite;
        private Sprite _defaultSprite;
        private Material _pHoldMaterial;
        private Material _qHoldMaterial;
        private Material _spaceHoldMaterial;

        private static readonly int OffsetProperty = Shader.PropertyToID("_Offset");
        private static readonly int TransitionTextureProperty = Shader.PropertyToID("_TransTex");

        private void Awake()
        {
            _pHoldMaterial = CreateHoldMaterialInstance(pKeyImage, pTransitionTexture);
            _qHoldMaterial = CreateHoldMaterialInstance(qKeyImage, qTransitionTexture);
            _spaceHoldMaterial = CreateHoldMaterialInstance(spaceKeyImage, spaceTransitionTexture);
            ResetHoldProgress();
        }

        private void Update()
        {
            if (_activeKey == Key.None || Keyboard.current == null)
            {
                return;
            }

            Image keyImage = GetImageForKey(_activeKey);
            if (keyImage == null)
            {
                return;
            }

            keyImage.sprite = Keyboard.current[_activeKey].isPressed
                ? _pushSprite
                : _defaultSprite;
        }

        public void Show(EButtonActionType actionType, Key key, float duration = 0f)
        {
            StopAnimation();
            HideAllImages();

            _activeImage = GetAnimationImageForKey(key);
            _activeKeyImage = GetImageForKey(key);
            if (_activeKeyImage == null)
            {
                return;
            }

            _activeKey = key;
            _pushSprite = GetPushSpriteForKey(key);
            _defaultSprite = _activeKeyImage.sprite;

            if (_activeImage != null && actionType != EButtonActionType.None)
            {
                _activeImage.gameObject.SetActive(true);
            }

            switch (actionType)
            {
                case EButtonActionType.SinglePress:
                    _animationCoroutine = StartCoroutine(RunLoopingFrameAnimation(singlePressFrames));
                    break;
                case EButtonActionType.RapidPress:
                    _animationCoroutine = StartCoroutine(RunLoopingFrameAnimation(pressFrames));
                    break;
                case EButtonActionType.Timing:
                    _animationCoroutine = StartCoroutine(RunOneShotFrameAnimation(pressFrames, duration));
                    break;
            }
        }

        public void Hide()
        {
            StopAnimation();
            ResetHoldProgress();

            if (_activeKeyImage != null && _defaultSprite != null)
            {
                _activeKeyImage.sprite = _defaultSprite;
            }

            HideAllImages();
            _activeImage = null;
            _activeKeyImage = null;
            _activeKey = Key.None;
            _pushSprite = null;
            _defaultSprite = null;
        }

        public void SetHoldProgress(Key key, float progress)
        {
            Material material = GetHoldMaterialForKey(key);
            if (material == null || !material.HasProperty(OffsetProperty))
            {
                return;
            }

            material.SetFloat(OffsetProperty, -Mathf.Clamp01(progress));
        }

        public void ResetHoldProgress()
        {
            SetMaterialOffset(_pHoldMaterial, 0f);
            SetMaterialOffset(_qHoldMaterial, 0f);
            SetMaterialOffset(_spaceHoldMaterial, 0f);
        }

        private Image GetImageForKey(Key key)
        {
            return key switch
            {
                Key.P => pKeyImage,
                Key.Q => qKeyImage,
                Key.Space => spaceKeyImage,
                _ => null
            };
        }

        private Image GetAnimationImageForKey(Key key)
        {
            return key switch
            {
                Key.P => pActionImage,
                Key.Q => qActionImage,
                Key.Space => spaceActionImage,
                _ => null
            };
        }

        private Sprite GetPushSpriteForKey(Key key)
        {
            return key switch
            {
                Key.P => pPushSprite,
                Key.Q => qPushSprite,
                Key.Space => spacePushSprite,
                _ => null
            };
        }

        private Material CreateHoldMaterialInstance(Image targetImage, Texture2D transitionTexture)
        {
            if (targetImage == null)
            {
                return null;
            }

            Material source = holdMaterial != null ? holdMaterial : targetImage.material;
            if (source == null)
            {
                return null;
            }

            Material instance = new(source)
            {
                name = $"{source.name} ({targetImage.name})"
            };

            if (transitionTexture != null && instance.HasProperty(TransitionTextureProperty))
            {
                instance.SetTexture(TransitionTextureProperty, transitionTexture);
            }

            targetImage.material = instance;
            return instance;
        }

        private Material GetHoldMaterialForKey(Key key)
        {
            return key switch
            {
                Key.P => _pHoldMaterial,
                Key.Q => _qHoldMaterial,
                Key.Space => _spaceHoldMaterial,
                _ => null
            };
        }

        private static void SetMaterialOffset(Material material, float offset)
        {
            if (material != null && material.HasProperty(OffsetProperty))
            {
                material.SetFloat(OffsetProperty, offset);
            }
        }

        private void OnDestroy()
        {
            Destroy(_pHoldMaterial);
            Destroy(_qHoldMaterial);
            Destroy(_spaceHoldMaterial);
        }

        private void HideAllImages()
        {
            pActionImage.gameObject.SetActive(false);
            qActionImage.gameObject.SetActive(false);
            spaceActionImage.gameObject.SetActive(false);
        }

        private void StopAnimation()
        {
            if (_animationCoroutine == null)
            {
                return;
            }

            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        private IEnumerator RunLoopingFrameAnimation(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                yield break;
            }

            int frameIndex = 0;
            while (true)
            {
                _activeImage.sprite = frames[frameIndex];
                frameIndex = (frameIndex + 1) % frames.Length;
                yield return new WaitForSeconds(frameInterval);
            }
        }

        private IEnumerator RunOneShotFrameAnimation(Sprite[] frames, float duration)
        {
            if (frames == null || frames.Length == 0 || duration <= 0f)
            {
                yield break;
            }

            float perFrameDuration = duration / frames.Length;
            for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
            {
                _activeImage.sprite = frames[frameIndex];
                yield return new WaitForSeconds(perFrameDuration);
            }
        }
    }
}
