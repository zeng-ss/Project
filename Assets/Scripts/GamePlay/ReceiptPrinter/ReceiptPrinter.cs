using UnityEngine;
using TMPro;

namespace CasualGame.CoffeeAway
{
    public class ReceiptPrinter : AttachPoolPrefabBase
    {
        [SerializeField] private TextMeshPro m_Text;

        public void UpdateText(string text) => m_Text.text = text;
    }
}