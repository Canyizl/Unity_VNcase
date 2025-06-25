using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplay : MonoBehaviour
{
    public Image image;

    public void Setup(Sprite sprite, Vector2 anchor, Vector2 scale)
    {
        image.sprite = sprite;
        RectTransform rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = scale;
        gameObject.SetActive(true);
    }
}
