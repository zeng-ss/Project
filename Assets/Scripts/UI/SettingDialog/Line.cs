// using UnityEngine;
// using System.Threading;
// using UnityEngine.UI;
// using DG.Tweening;

// namespace CasualGame
// {
//     public enum Media
//     {
//         Music,
//         Sound,
//         Vibrate,
//     }
//     public class Line : MonoBehaviour
//     {
//         // AutoBindFieldInfo
//         [Header("PrefabBinderTool")]
//         [SerializeField] private TMPro.TextMeshProUGUI m_title;

//         [SerializeField] private UnityEngine.UI.Image m_dotIcon;

//         [SerializeField] private UIGray m_UIGray;

//         [SerializeField] private UnityEngine.UI.Button m_dotBtn;

//         // AutoBindFieldInfo

//         private CancellationTokenSource _cts;
//         private bool isMuted;
//         private Media media;
//         private RectTransform m_dotRectTransform;
//         private Sprite _curSprite;
//         private const float offset = 68f;

//         public async void SetData(Media media)
//         {
//             _cts = new CancellationTokenSource();
//             m_dotBtn.onClick.RemoveAllListeners();
//             m_dotBtn.onClick.AddListener(OnClick_DotBtn);

//             this.media = media;
//             this.isMuted = ProxyManager.SpotProxy.IsMediaMuted((int)media);
//             this.m_dotRectTransform = m_dotBtn.GetComponent<RectTransform>();
//             Refresh();

//             m_title.text = GameEntry.Localization.GetString($"{media + 7}");

//             _curSprite = await AtlasPool.Instance.Spawn(AtlasId.Setting, $"icon{media}", _cts.Token);
//             m_dotIcon.sprite = _curSprite;
//             m_dotIcon.SetNativeSize();
//         }

//         public void ReleaseData()
//         {
//             m_dotBtn.transform.DOKill();
//             if (_curSprite != null)
//             {
//                 AtlasPool.Instance.DeSpawn(_curSprite);
//                 _curSprite = null;
//             }
//         }

//         private void OnClick_DotBtn()
//         {
//             PlaySoundManager.Instance.PlaySound(Constant.Audio.click_start);
//             LogHandler.Info($"OnClick_DotBtn: {media}", LogColor.Green);
//             ProxyManager.SpotProxy.SetMediaMuted((int)media, !isMuted);
//             isMuted = !isMuted;
//             m_dotRectTransform.DOAnchorPosX(isMuted ? -offset : offset, 0.2f).SetEase(isMuted ? Ease.InBack : Ease.OutBack).OnComplete(Refresh);
//         }

//         private void Refresh()
//         {
//             m_dotRectTransform.anchoredPosition = new Vector2(isMuted ? -offset : offset, m_dotRectTransform.anchoredPosition.y);
//             m_UIGray.IsGray = isMuted;
//         }
//     }
// }