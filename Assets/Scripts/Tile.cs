using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region Variables

    public Vector2 pos;
    public Vector2 value = Vector2.zero; //x là độ dài block, y là thứ tự trong block
    #endregion

    #region Mono

    private void Awake()
    {
        SetUpPos();
    }

    #endregion

    #region Methods

    private void SetUpPos()
    {
        var localPosition = transform.localPosition;
        pos.x = localPosition.x;
        pos.y = localPosition.y;
    }

    #endregion
}