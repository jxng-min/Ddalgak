using JxModule;
using UnityEngine.EventSystems;

namespace Ddalgak
{
    public class TitleView : ViewBase
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            SceneLoadManager.Instance.LoadScene(ESceneType.GameScene);
        }
    }
}