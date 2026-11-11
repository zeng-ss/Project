using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace CasualGame
{
    public class Mutils
    {
        public static class Config
        {
            // normal
            public const float Spacing = 1.2f;
            public const float ItemOffset = 0.2f;
            public const int CellLockNum = 4;
            public const int CellAddTime = 30;
            public const int TimeBase = 25;
            public const int ScoreBase = 1;
            public const int ComboEffectMin = 3;
            public const int IceTime = 15;
            // outside
            public const int UnlockLevel = 10;
            public const int AddTime = 30;
            public const float StarRate = 2;
            public const int HammerTime = 4;
            // layer
            public const string GoodsSelfLayer = "GoodsSelf";
            public static readonly int GoodsSelfLayerId = LayerMask.NameToLayer(GoodsSelfLayer);
            public const string GoodsItemLayer = "GoodsItem";
            public static readonly int GoodsItemLayerId = LayerMask.NameToLayer(GoodsItemLayer);
            public const string GoodsLockLayer = "GoodsLock";
            public static readonly int GoodsLockLayerId = LayerMask.NameToLayer(GoodsLockLayer);
            // Sorting layer
            public const string SortingLayer_Default = "Default";
            public static readonly int SortingLayerId_Default = SortingLayer.NameToID(SortingLayer_Default);
            public const string SortingLayer_ShelveCell = "ShelveCell";
            public static readonly int SortingLayerId_ShelveCell = SortingLayer.NameToID(SortingLayer_ShelveCell);
            public const string SortingLayer_Effect = "Effect";
            public static readonly int SortingLayerId_Effect = SortingLayer.NameToID(SortingLayer_Effect);
            public const string SortingLayer_CasualUI = "CasualUI";
            public static readonly int SortingLayerId_CasualUI = SortingLayer.NameToID(SortingLayer_CasualUI);
            public const string SortingLayer_HighLight = "HighLight";
            public static readonly int SortingLayerId_HighLight = SortingLayer.NameToID(SortingLayer_HighLight);
        }

        public static class Tool
        {
        }

        public static class Animation
        {
            public static Sequence DuangOnce(Transform item, float duangTime = 0.1f)
            {
                item.localScale = Vector3.one;
                Sequence Sequence = DOTween.Sequence();
                Sequence.Append(item.DOScale(new Vector3(1.05f, 0.95f, 1.05f), duangTime).SetEase(Ease.Linear))
                    .Append(item.DOScale(new Vector3(0.95f, 1.05f, 0.95f), duangTime).SetEase(Ease.Linear))
                    .Append(item.DOScale(new Vector3(1, 1, 1), duangTime).SetEase(Ease.Linear));
                return Sequence;
            }

            public static Sequence DuangJump(Transform item, float duangTime = 0.1f)
            {
                item.localScale = Vector3.one;
                Sequence Sequence = DOTween.Sequence();
                Sequence.Append(item.DOScale(new Vector3(1.1f, 0.9f, 1.1f), duangTime).SetEase(Ease.Linear))
                    .Append(item.DOScale(new Vector3(1, 1, 1), duangTime).SetEase(Ease.Linear));
                return Sequence;
            }

            public static Sequence DuangRecycle(Transform item, float duangTime = 0.1f)
            {
                item.localScale = Vector3.one;
                Sequence Sequence = DOTween.Sequence();
                Sequence.Append(item.DOScale(new Vector3(1.1f, 0.9f, 1.1f), duangTime).SetEase(Ease.Linear))
                    .Append(item.DOScale(new Vector3(0.9f, 1.1f, 0.9f), duangTime).SetEase(Ease.Linear))
                    .Join(item.DOLocalMoveY(item.localPosition.y + 0.3f, duangTime).SetEase(Ease.Linear));
                return Sequence;
            }

            public static Tween FlyScale(Transform item)
            {
                item.transform.localScale = Vector3.one;
                item.transform.DOKill();
                Tween tween = item.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
                  {
                      item.transform.DOScale(1f, 0.1f);
                  });
                return tween;
            }

            private static readonly AnimationCurve extremeCurve = new AnimationCurve(
                new Keyframe(0f, 1.5f),
                new Keyframe(0.08f, -1.5f),
                new Keyframe(0.16f, 1.2f),
                new Keyframe(0.24f, -1f),
                new Keyframe(0.32f, 0.8f),
                new Keyframe(0.5f, -0.6f),
                new Keyframe(0.7f, 0.4f),
                new Keyframe(0.85f, -0.2f),
                new Keyframe(1f, 0f)
            );

            public static void ShakeExtreme(Transform target, float duration = 0.8f, float strength = 1.5f)
            {
                ShakeWithCurve(target, duration, strength, extremeCurve);
            }

            private static void ShakeWithCurve(Transform target, float duration, float strength, AnimationCurve curve)
            {
                target.DOKill(true);
                Vector3 origPos = target.localPosition;

                DOVirtual.Float(0f, 1f, duration, t =>
                {
                    float curveValue = curve.Evaluate(t);
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    // float timeMultiplier = Mathf.Sin(t * Mathf.PI * 6f) * 0.3f + 1f;
                    Vector3 offset = new Vector3(randomDir.x, randomDir.y, 0) * (strength * curveValue/* * timeMultiplier*/);
                    target.localPosition = origPos + offset;
                }).OnComplete(() =>
                {
                    target.localPosition = origPos;
                });
            }

            public static Sequence ShowFromDown(GameObject gameObject, float duration = 0.5f)
            {
                gameObject.SetActive(false);
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                float anchoredY = rectTransform.anchoredPosition.y;
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y - 50);
                MaskableGraphic[] images = gameObject.GetComponentsInChildren<MaskableGraphic>();
                float[] originAlphaImages = new float[images.Length];
                for (int i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    originAlphaImages[i] = image.color.a;
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                }
                Sequence sequence = DOTween.Sequence();
                sequence.AppendCallback(() => { gameObject.SetActive(true); })
                    .Append(rectTransform.DOAnchorPosY(anchoredY, duration).SetEase(Ease.OutSine));
                for (int i = 0; i < images.Length; i++)
                {
                    sequence.Join(images[i].DOFade(originAlphaImages[i], duration));
                }

                sequence.OnComplete(() =>
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        var image = images[i];
                        image.color = new Color(image.color.r, image.color.g, image.color.b, originAlphaImages[i]);
                    }
                });
                return sequence;
            }

            public static Sequence ScaleFromBig(GameObject gameObject, RectTransform parent, float duration = 0.5f)
            {
                gameObject.SetActive(false);
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                MaskableGraphic[] images = gameObject.GetComponentsInChildren<MaskableGraphic>();
                float[] originAlphaImages = new float[images.Length];
                for (int i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    originAlphaImages[i] = image.color.a;
                }

                Sequence sequence = DOTween.Sequence();
                sequence.AppendCallback(() =>
                    {
                        for (int i = 0; i < images.Length; i++)
                        {
                            var image = images[i];
                            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                        }
                        rectTransform.localScale = Vector3.one * 1.5f;
                        gameObject.SetActive(true);
                    })
                    .Append(rectTransform.DOScale(1, duration).SetEase(Ease.InCubic));
                for (int i = 0; i < images.Length; i++)
                {
                    sequence.Join(images[i].DOFade(originAlphaImages[i], duration));
                }
                if (parent != null)
                {
                    sequence.Append(parent.DOShakePosition(0.3f, 15f, 20));
                }

                sequence.OnComplete(() =>
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        var image = images[i];
                        image.color = new Color(image.color.r, image.color.g, image.color.b, originAlphaImages[i]);
                    }
                });

                return sequence;
            }
        }
    }
}