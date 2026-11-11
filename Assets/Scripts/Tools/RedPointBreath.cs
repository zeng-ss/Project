using UnityEngine;
using DG.Tweening;

namespace CasualGame
{
    public class RedPointBreath : MonoBehaviour
    {
        void Awake()
        {
            gameObject.GetOrAddComponent<AutoBreath>();
        }
    }
}