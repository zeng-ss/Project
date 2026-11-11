using DG.Tweening;
using TMPro;
using UnityEngine;

public abstract class UIFormTextViewBase
{
    public UIFormTextViewBase(UIFormTextSo uIFormTextSo, TMP_Text tMP_Text)
    {
        TMP_Text = tMP_Text;
        UIFormTextSo = uIFormTextSo;
    }
    public TMP_Text TMP_Text;
    public UIFormTextSo UIFormTextSo;
    public abstract void PlayText(Sequence sequence);
    public abstract void ResetDefault();
}
