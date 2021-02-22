using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Variables

    public Vector2 pos;

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