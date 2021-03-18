﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Lean.Common;
using Lean.Transition.Method;
using Lean.Pool;
using Shapes2D;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class Block : MonoBehaviour, IPoolable
{
    #region Variable

    [SerializeField] private int blockLength;
    [SerializeField] public bool isOnBoard = false;
    public Vector2 pos;
    [SerializeField] private BlockTranslate translateComponent;
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private int leftBlankLength = 0;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int rightBlankLength = 0;

    //[SerializeField] private TextMeshProUGUI posText;
    [SerializeField] private Shape shape2d;
    public bool isRainbow = false;
    private List<Vector2> nearbyBlock = new List<Vector2>();

    #endregion

    #region Mono

    public void OnSpawn()
    {
        GameEvents.Instance.OnBlockMoveUp += MoveUp;
        GameEvents.Instance.OnBlockMoveDown += MoveDown;
        GameEvents.Instance.OnFindLimitArea += FindLimitArea;
        GameEvents.Instance.OnBlockExplode += Explode;
        GameEvents.Instance.OnFindNearByBlocks += FindNearbyBlocks;
        GameEvents.Instance.OnRainbowBlockAnimation += StartRainbowAnimation;
        shape2d.settings.fillColor =
            BoardManager.Instance.blockColors[Random.Range(0, BoardManager.Instance.blockColors.Length)];
        FindLimitArea();
    }

    public void OnDespawn()
    {
        GameEvents.Instance.OnBlockMoveUp -= MoveUp;
        GameEvents.Instance.OnBlockMoveDown -= MoveDown;
        GameEvents.Instance.OnFindLimitArea -= FindLimitArea;
        GameEvents.Instance.OnBlockExplode -= Explode;
        GameEvents.Instance.OnFindNearByBlocks -= FindNearbyBlocks;
        GameEvents.Instance.OnRainbowBlockAnimation -= StartRainbowAnimation;
        transform.parent = null;
        isOnBoard = false;
        if (isRainbow)
        {
            BoardManager.Instance.hasRainbowBlock = false;
        }

        transform.localScale = new Vector3(blockLength, 1, 1);
    }

    private void Awake()
    {
        shape2d = GetComponent<Shape>();
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

            Vector3 des = new Vector3(transform.position.x, pos.y + 1, -2);
            transform.parent = BoardManager.Instance.gridGameObjects[(int) pos.x, (int) pos.y + 1].transform;
            pos.y++;

            LeanTween.move(gameObject, des, AnimationManager.Instance.moveUpTime).setEase(AnimationManager
                .Instance.moveUpTween).setOnComplete(() =>
            {
                FindLimitArea();
                FindNearbyBlocks();
            });

            //debug color
            if (isRainbow)
            {
                BoardManager.Instance.rainbowPos = pos;
                shape2d.settings.fillColor = Color.black;
            }
        }
    }

    private void MoveDown(Vector2 calledPos, int step)
    {
        if (new Vector2((int) calledPos.x, (int) calledPos.y) == pos && pos.y > 0)
        {
            pos.y -= step;

            if (isRainbow)
            {
                BoardManager.Instance.rainbowPos = pos;
            }

            Vector2 newPos = new Vector2(transform.position.x, pos.y);
            Vector3 des = new Vector3(newPos.x, pos.y, transform.position.z);
            transform.parent = null;
            transform.parent = BoardManager.Instance.gridGameObjects[(int) pos.x, (int) pos.y].transform;

            LeanTween.move(gameObject, des, AnimationManager.Instance.moveDownTime).setEase(AnimationManager
                .Instance.moveDownTween).setOnComplete(() => { });
        }

        Invoke("FindLimitArea", AnimationManager.Instance.moveDownTime);
    }

    private void Explode(Vector2 calledPos, bool hasFullRowRainbow)
    {
        if (calledPos == pos)
        {
            StartCoroutine(ExplodeCoroutine());
        }

        IEnumerator ExplodeCoroutine()
        {
            if (hasFullRowRainbow)
            {
                yield return new WaitForSeconds(AnimationManager.Instance.rainbowExplodeTime);
            }

            shape2d.settings.fillColor = Color.white;
            if (!isRainbow)
            {
                LeanTween.scale(gameObject, Vector3.zero, AnimationManager.Instance.explodeTime).setEase
                    (AnimationManager.Instance.explodeTween).setOnComplete(() => { LeanPool.Despawn(gameObject); });
            }
            else
            {
                FindNearbyBlocks();
                for (int i = 0; i < nearbyBlock.Count; i++)
                {
                    Vector2 pos = nearbyBlock[i];
                    if (pos != null && pos != new Vector2(-5, -5))
                    {
                        BoardManager.Instance.DeleteBlock(pos);
                        GameEvents.Instance.BlockExplode(pos, false);
                    }
                }

                LeanTween.scale(gameObject, Vector3.zero, AnimationManager.Instance.explodeTime).setEase
                    (AnimationManager.Instance.explodeTween).setOnComplete(() =>
                {
                    BoardManager.Instance.ScanMoveDown(true);
                    BoardManager.Instance.hasRainbowBlock = false;
                    nearbyBlock.Clear();

                    LeanPool.Despawn(gameObject);
                });
            }

            yield return null;
        }
    }

    private void FindNearbyBlocks()
    {
        if (isOnBoard && isRainbow)
        {
            nearbyBlock = BoardManager.Instance.GetNearbyBlocks(pos, blockLength);
            if (nearbyBlock.Count == 0)
            {
                nearbyBlock.Add(new Vector2(-5, -5)); // not exist
            }
        }
    }

    public void FindLimitArea() // when block is first selected
    {
        if (isOnBoard && pos.x < 8 && pos.y < 10)
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
            BoardManager.Instance.canDrag = false;
            GameObject matchedTile = BoardManager.Instance.CheckDropPos(anchorPoint.transform, (int) pos.y);
            Vector2 oldPos = pos;
            if (matchedTile)
            {
                pos = matchedTile.GetComponent<Tile>().pos;
                if (oldPos != pos)
                {
                    BoardManager.Instance.DragBlockFingerUp(oldPos, pos, blockLength);
                    Vector3 newPos = matchedTile.transform.position;
                    Vector3 des = new Vector3(newPos.x + (0.5f * (blockLength - 1)), newPos.y, -2);
                    LeanTween.move(gameObject, des, AnimationManager.Instance.moveToTileTime).setEase(AnimationManager
                        .Instance.moveToTileTween).setOnComplete(() =>
                    {
                        transform.parent = matchedTile.transform;
                        BoardManager.Instance.hasMovedUp = false;
                        BoardManager.Instance.ScanMoveDown(true);
                    });
                }
                else
                {
                    Vector3 des =
                        new Vector3(oldPos.x + (0.5f * (blockLength - 1)), oldPos.y, -2); //ve vi tri cu
                    LeanTween.move(gameObject, des, AnimationManager.Instance.moveToTileTime).setEase(AnimationManager
                        .Instance.moveToTileTween).setOnComplete(() => { BoardManager.Instance.canDrag = true; });
                }
            }
            else
            {
                Vector3 des = new Vector3(oldPos.x + (0.5f * (blockLength - 1)), oldPos.y, -2); //ve vi tri cu
                LeanTween.move(gameObject, des, AnimationManager.Instance.moveToTileTime).setEase(AnimationManager
                    .Instance.moveToTileTween);
            }
        }
    }

    public void StartRainbowAnimation(Vector2 _pos)
    {
        if (pos == _pos)
        {
            Debug.Log("rainbow anim");
            StartCoroutine(Blink(AnimationManager.Instance.rainbowExplodeTime));

            IEnumerator Blink(float waitTime)
            {
                Color32 tempColor = shape2d.settings.fillColor;
                float endTime = Time.time + waitTime;
                while (Time.time < endTime)
                {
                    spriteRenderer.enabled = false;
                    yield return new WaitForSeconds(0.1f);
                    spriteRenderer.enabled = true;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    #endregion
}