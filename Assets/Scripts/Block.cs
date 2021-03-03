﻿using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Transition.Method;
using Lean.Pool;
using UnityEngine;

public class Block : MonoBehaviour, IPoolable
{
    #region Variable

    [SerializeField] private int blockLength;
    [SerializeField] private bool isOnBoard = false;
    public Vector2 pos;
    [SerializeField] private BlockTranslate translateComponent;
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private int leftBlankLength = 0;
    [SerializeField] private int rightBlankLength = 0;
    private Transform currentTransform;

    #endregion

    #region Mono

    public void OnSpawn()
    {
        GameEvents.Instance.OnBlockMoveUp += MoveUp;
        currentTransform = transform;
    }

    public void OnDespawn()
    {
        GameEvents.Instance.OnBlockMoveUp -= MoveUp;
    }

    #endregion

    #region Methods

    private void MoveUp()
    {
        if ((int) pos.y >= 9)
        {
            LeanPool.Despawn(gameObject);
        }
        else
        {
            if (!isOnBoard)
            {
                isOnBoard = true;
            }

            transform.position = new Vector3(transform.position.x, transform.position.y + 1, -2);
            transform.parent = BoardManager.Instance.gridGameObjects[(int) pos.x, (int) pos.y + 1].transform;
            pos.y++;
            currentTransform = transform;
            FindLimitArea();
        }
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
                    translateComponent.leftLimit = transform.position.x - leftBlankLength;
                }
                else
                {
                    translateComponent.leftLimit = transform.position.x;
                }

                if (rightBlankLength != 0)
                {
                    translateComponent.rightLimit = transform.position.x + rightBlankLength;
                }
                else
                {
                    translateComponent.rightLimit = transform.position.x;
                }
            }
        }
    }

    public void DragFingerUpToTile()
    {
        if (translateComponent.enabled)
        {
            GameObject matchedTile = BoardManager.Instance.CheckDropPos(anchorPoint.transform, (int) pos.y);
            Vector2 oldPos = pos;
            if (matchedTile)
            {
                pos = matchedTile.GetComponent<Tile>().pos;
                if (oldPos != pos)
                {
                    BoardManager.Instance.DragBlockFingerUp(oldPos, pos, blockLength);
                    Vector3 newPos = matchedTile.transform.position;
                    transform.position = new Vector3(newPos.x + (0.5f * (blockLength - 1)), newPos.y, currentTransform.position.z);
                    transform.parent = matchedTile.transform;
                    //goi scan move down 
                }
                else
                {
                    transform.position = new Vector3(oldPos.x + (0.5f * (blockLength - 1)), oldPos.y, currentTransform.position.z); //ve vi tri cu
                }
            }
            else
            {
                transform.position = new Vector3(oldPos.x + (0.5f * (blockLength - 1)), oldPos.y, currentTransform.position.z); //ve vi tri cu
            }
        }
    }

    #endregion
}