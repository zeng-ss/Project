//------------------------------------------------------------
// File : AspectFullImage.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace SortGame.Core
{
    public class AspectFullImage : MonoBehaviour
    {
        private bool initImage;
        private Image image;
        private AspectRatioFitter aspectRatioFitter;

        private void Awake()
        {
            Refresh();
        }

        public void Refresh()
        {
            image = GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                image.preserveAspect = false;
                RectTransform rectTransform = image.rectTransform;
                // rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                aspectRatioFitter = gameObject.GetOrAddComponent<AspectRatioFitter>();
                if (aspectRatioFitter != null)
                {
                    initImage = true;
                    aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                    aspectRatioFitter.aspectRatio = (float)image.sprite.rect.width / image.sprite.rect.height;
                }
            }
        }
    }
}