using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class TextMeshMaterialState : StateHandler
{
    private TMPro.TextMeshProUGUI text;
    public Material[] material;

    public override void SetState(int status)
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        if (text == null || status < 0 || status >= material.Length)
            return;
        text.fontMaterial = material[status];
    }
}
