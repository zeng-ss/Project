using UnityEngine;
using UnityEngine.UI;
using System;

namespace CasualGame
{
    public class GameButton : MonoBehaviour, IEffectTarget
    {
        public Action OnClick;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => OnClick?.Invoke());
        }

        public void PlayParticle()
        {
        }

        public Transform GetTransform() => transform;
    }
}