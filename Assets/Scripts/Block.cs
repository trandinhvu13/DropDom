﻿using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Variable

    [SerializeField] private int blockLength;
    [SerializeField] private bool isOnBoard = false;
    public Vector2 pos;
    [SerializeField] private BlockTranslate translateComponent;
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private int leftBlankLength = 0;
    [SerializeField] private int rightBlankLength = 0;

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

        transform.position = new Vector3(transform.position.x, transform.position.y + 1, -2);
        transform.parent = BoardManager.Instance.gridGameObjects[(int) pos.x, (int) pos.y + 1].transform;
        pos.y++;
        FindLimitArea();
    }

    public void FindLimitArea() // when block is first selected
    {
        if (isOnBoard)
        {
            int posX = (int) pos.x;
            int posY = (int) pos.y;

            leftBlankLength = BoardManager.Instance.ReturnBlankLength(posX, posY, blockLength, "left");
            rightBlankLength = BoardManager.Instance.ReturnBlankLength(posX, posY, blockLength, "right");

            if (leftBlankLength == 0 && rightBlankLength == 0)
            {
                translateComponent.enabled = false;
            }
            else
            {
                translateComponent.enabled = true;
                if (leftBlankLength != 0)
                {
                    translateComponent.leftLimit = pos.x - leftBlankLength;
                }
                else
                {
                    translateComponent.leftLimit = pos.x;
                }

                if (rightBlankLength != 0)
                {
                    translateComponent.rightLimit = pos.x + rightBlankLength;
                }
                else
                {
                    translateComponent.rightLimit = pos.x;
                }

                translateComponent.leftLimit = leftBlankLength;
                translateComponent.rightLimit = rightBlankLength;
            }
        }
    }

    #endregion
}