using JxModule.DataTable;
using UnityEngine;
using JxModule;

namespace Ddalgak
{
    public class EndingDataTableRow : DataTableRowBase
    {
        public EGameOverReason gameOverReason;
        public string title;
        public string presentation;
        public Texture2D endingImage;
        public string endingText;

        public GameOverPresentationData ToRuntimeData()
        {
            return new GameOverPresentationData
            {
                reason = gameOverReason,
                title = title,
                presentation = presentation,
                message = endingText,
                image = endingImage != null ? endingImage.ToSprite() : null
            };
        }
    }
}
