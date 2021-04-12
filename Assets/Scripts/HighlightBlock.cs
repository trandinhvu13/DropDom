using System;
using System.Collections;
using System.Collections.Generic;
using Shapes2D;
using UnityEngine;

public class HighlightBlock : MonoBehaviour
{
    [SerializeField] private int column;
    [SerializeField] private Color32 normalColor;
    [SerializeField] private Color32 highlightColor;
    [SerializeField] private bool isHighlighted = false;
    public Shape shape2d;

    private void OnEnable()
    {
        GameEvents.Instance.OnHighlightBlock += ChangeToHighlightColor;
        GameEvents.Instance.OnDehighlightBlock += ChangeToNormalColor;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnHighlightBlock -= ChangeToHighlightColor;
        GameEvents.Instance.OnDehighlightBlock -= ChangeToNormalColor;
    }

    public void ChangeToHighlightColor(int x)
    {
        if (column != x) return;
        if (!isHighlighted)
        {
            isHighlighted = true;
            shape2d.settings.fillColor = highlightColor;
        }
    }

    public void ChangeToNormalColor(int x)
    {
        if (column != x) return;
        if (isHighlighted)
        {
            isHighlighted = false;
            shape2d.settings.fillColor = normalColor;
        }
    }
}