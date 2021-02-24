using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Variable

    public int blockLength;
    [SerializeField] private bool isOnBoard = false;
    public Vector2 pos;

    #endregion

    #region Mono

    private void OnEnable()
    {
        GameEvents.Instance.OnBlockMoveUp += MoveUp;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnBlockMoveUp -= MoveUp;
    }

    #endregion

    #region Methods

    private void MoveUp()
    {
        if (!isOnBoard)
        {
            isOnBoard = true;
        }
        pos = new Vector2(pos.x + 1, pos.y + 1);
        transform.position = new Vector3(transform.position.x, transform.position.y + 1);
        //change parent
        transform.parent = BoardManager.Instance.gridGameObjects[(int)pos.x, (int)pos.y].transform;
    }

    #endregion
}