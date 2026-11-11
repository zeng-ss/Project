using DG.Tweening;
using UnityEngine;

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
		}

		public void OnUpdate(float elapseSeconds, float realElapseSeconds)
		{

		}

		public void OnClose()
		{
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