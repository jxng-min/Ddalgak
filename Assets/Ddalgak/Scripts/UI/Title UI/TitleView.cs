using JxModule;
using UnityEngine.EventSystems;

namespace Ddalgak
{
    public class TitleView : ViewBase
    {
        private void Awake()
        {
            SoundManager.Instance.PlayBgm("BGM_Title");
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            SoundManager.Instance.PlaySfx("SFX_ButtonNormalClick");
            SceneLoadManager.Instance.LoadScene(ESceneType.GameScene);
        }
    }
}