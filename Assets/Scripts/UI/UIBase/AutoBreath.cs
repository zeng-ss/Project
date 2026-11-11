//------------------------------------------------------------
// File : AutoBreath.cs
// Desc : 
//------------------------------------------------------------
using DG.Tweening;
using UnityEngine;

namespace CasualGame
{
    public class AutoBreath : MonoBehaviour
    {
        public bool autoEnable = true;
        public float rate = 1.1f;
        public float dutrion = 0.5f;
        private void OnEnable()
        {
            if (autoEnable)
                EnableBreathAnim();
        }


        void OnDisable()
        {
            transform.DOKill();
        }

        public void EnableBreathAnim()
        {
            transform.BreathAnim(dutrion, rate);
        }
    }
}