using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RebuildLayoutOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(RebuildLayout());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    IEnumerator RebuildLayout()
    {
        yield return new WaitForEndOfFrame();
        foreach (ContentSizeFitter contentSizeFitter in GetComponentsInChildren<ContentSizeFitter>())
        {
            RectTransform rectTransform = contentSizeFitter.gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }
        Canvas.ForceUpdateCanvases();
        yield break;
    }
}
