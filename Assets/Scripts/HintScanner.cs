using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HintScanner : MonoBehaviour
{
    struct MoveableBlock
    {
        public int leftBlank;
        public int rightBlank;
        public Vector2 pos;
        public int blockLength;
    }

    struct HintBlock
    {
        public Vector2 pos;
        public string dir;
        public int step;
    }

    private int[,] gridValueDuplicate = new int[8, 10];
    private List<MoveableBlock> moveableBlocks = new List<MoveableBlock>();
    private HintBlock hint = new HintBlock();

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    IEnumerator CheckForHint()
    {
        //scan grid
        gridValueDuplicate = BoardManager.Instance.gridValue;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                if (gridValueDuplicate[x, y] == 11 || gridValueDuplicate[x, y] == 21 ||
                    gridValueDuplicate[x, y] == 31 || gridValueDuplicate[x, y] == 41)
                {
                    int blockLength = (int) gridValueDuplicate[x, y] / 10;
                    int leftBlankLength = BoardManager.Instance.ReturnBlankLength(x, y, blockLength, "left");
                    int rightBlankLength = BoardManager.Instance.ReturnBlankLength(x, y, blockLength, "right");

                    if (leftBlankLength != 0 || rightBlankLength != 0)
                    {
                        MoveableBlock moveableBlock = new MoveableBlock();
                        moveableBlock.pos = new Vector2(x, y);
                        moveableBlock.leftBlank = leftBlankLength;
                        moveableBlock.rightBlank = rightBlankLength;
                        moveableBlock.blockLength = blockLength;
                        moveableBlocks.Add(moveableBlock);
                    }
                }
            }
        }

        //check each moveable block
        for (int i = 0; i < moveableBlocks.Count; i++)
        {
            //simulate drop
        }

        yield return null;
    }

    private bool SimulateDrop(MoveableBlock block)
    {
        bool hasHint = false;
        int[,] grid = gridValueDuplicate;
        Vector2 oldPos = block.pos;

        //check left blank
        if (block.leftBlank > 0)
        {
            for (int step = 1; step <= block.leftBlank; step++)
            {
                for (int i = 0; i < block.blockLength; i++)
                {
                    grid[(int) oldPos.x + i, (int) oldPos.y] = 0;
                }

                for (int i = 1; i <= block.blockLength; i++)
                {
                    grid[(int) oldPos.x - step + i - 1, (int) oldPos.y] = block.blockLength * 10 + i;
                }

                if (ScanMoveDown(grid, true))
                {
                    hint.dir = "left";
                    hint.pos = new Vector2((int) oldPos.x - step, (int) oldPos.y);
                    hint.step = step;
                    hasHint = true;
                    break;
                }
            }
        }
        
        //if left blanks no answer -> right blanks
        if (!hasHint)
        {
            if (block.rightBlank > 0)
            {
                for (int step = 1; step <= block.leftBlank; step++)
                {
                    for (int i = 0; i < block.blockLength; i++)
                    {
                        grid[(int) oldPos.x + i, (int) oldPos.y] = 0;
                    }

                    for (int i = 1; i <= block.blockLength; i++)
                    {
                        grid[(int) oldPos.x + step + i - 1, (int) oldPos.y] = block.blockLength * 10 + i;
                    }

                    if (ScanMoveDown(grid, true))
                    {
                        hint.dir = "left";
                        hint.pos = new Vector2((int) oldPos.x - step, (int) oldPos.y);
                        hint.step = step;
                        hasHint = true;
                        break;
                    }
                }
            }
        }

        return hasHint;
    }

    public bool ScanMoveDown(int[,] gridValue, bool isContinue)
    {
        bool isFoundFullRow = false;
        if (isContinue)
        {
            isContinue = false;

            for (int y = 1; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    switch (gridValue[x, y])
                    {
                        case 11:
                        {
                            if (gridValue[x, y - 1] == 0)
                            {
                                MoveDown(x, y, 1);
                            }

                            break;
                        }
                        case 21:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0)
                            {
                                MoveDown(x, y, 2);
                            }

                            break;
                        }
                        case 31:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0 &&
                                gridValue[x + 2, y - 1] == 0)
                            {
                                MoveDown(x, y, 3);
                            }

                            break;
                        }
                        case 41:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0 &&
                                gridValue[x + 2, y - 1] == 0 &&
                                gridValue[x + 3, y - 1] == 0)
                            {
                                MoveDown(x, y, 4);
                            }

                            break;
                        }
                    }
                }
            }

            ScanMoveDown(gridValue, isContinue);
        }
        else
        {
            //check full row
            bool isRowFull = true;
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (gridValue[x, y] == 0)
                    {
                        isRowFull = false;
                    }
                }

                if (isRowFull)
                {
                    isFoundFullRow = true;
                }
            }
        }

        void MoveDown(int x, int y, int length)
        {
            isContinue = true;
            for (int i = 1; i <= length; i++)
            {
                gridValue[x + i - 1, y] = 0;
                gridValue[x + i - 1, y - 1] = length * 10 + i;
            }
        }

        return isFoundFullRow;
    }
}