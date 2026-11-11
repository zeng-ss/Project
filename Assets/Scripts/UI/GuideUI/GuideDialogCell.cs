using DG.Tweening;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework.Event;

namespace CasualGame
{
	public class GuideDialogCell : MonoBehaviour, IUGUIChild
	{
		// AutoBindFieldInfo
		[Header("PrefabBinderTool")]

		[SerializeField] private TMPro.TextMeshProUGUI NameText_TextMeshProUGUI;

		[SerializeField] private TMPro.TextMeshProUGUI DialogText_TextMeshProUGUI;

		// AutoBindFieldInfo

		private Sequence _sequence;

		public void ShowDialog(DRDialogGuide drDialogGuide)
		{

		}

		public void OnInit()
		{

		}

		public void OnOpen()
		{
			GameEntry.Event.Subscribe(PlaySoundSuccessEventArgs.EventId, PlaySoundSuccessEvent);
		}

		public void OnUpdate(float elapseSeconds, float realElapseSeconds)
		{

		}

		private void PlaySoundSuccessEvent(object sender, GameEventArgs e)
		{
			PlaySoundSuccessEventArgs playSoundSuccessEventArgs = (PlaySoundSuccessEventArgs)e;

			// if (playSoundSuccessEventArgs.SerialId == _currentAudioID)
			// {
			//  _sequence = ItemAnimation.TypeWrite(m_TextContent, _contentText, playSoundSuccessEventArgs.SoundAgent.Length - 0.6f);
			// }
		}

		public void OnClose()
		{
			GameEntry.Event.Unsubscribe(PlaySoundSuccessEventArgs.EventId, PlaySoundSuccessEvent);

			if (_sequence.IsActive())
			{
				_sequence.Kill();
			}
		}

		public void OnSecondUpdate(float time)
		{

		}
	}
}