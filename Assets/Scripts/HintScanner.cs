using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UIElements;

public class HintScanner : MonoBehaviour
{
    struct MoveableBlock
    {
        public void IncreaseYPos()
        {
            pos.y++;
        }

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
    private IEnumerator CheckHint;
    private bool isFoundFullRow = false;
    [SerializeField] private float delayTime;

    private void OnEnable()
    {
        GameEvents.Instance.OnStartHintScan += () =>
        {
            CheckHint = CheckForHint();
            StartCoroutine(CheckForHint());
        };
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnStartHintScan -= () =>
        {
            CheckHint = CheckForHint();
            StartCoroutine(CheckForHint());
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            CheckHint = CheckForHint();
            StartCoroutine(CheckHint);
        }
    }

    IEnumerator CheckForHint()
    {
        Debug.Log("Check hint");
        //TH 1:
        for (int x = 0; x < 8; x++)
        {
            gridValueDuplicate[x, 0] = BoardManager.Instance.standbyRowValue[x];
            gridValueDuplicate[x, 1] = BoardManager.Instance.gridValue[x, 0];
        }

        for (int x = 0; x < 8; x++)
        {
            if (gridValueDuplicate[x, 1] == 11 || gridValueDuplicate[x, 1] == 21 || gridValueDuplicate[x, 1] == 31 ||
                gridValueDuplicate[x, 1] == 41)
            {
                int blockLength = (int)gridValueDuplicate[x, 1] / 10;
                bool isFullRow = true;
                for (int i = 0; i < blockLength; i++)
                {
                    if (gridValueDuplicate[x + i, 0] != 0)
                    {
                        isFullRow = false;
                    }
                }

                if (isFullRow)
                {
                    Debug.Log("no need");
                    yield break;
                }
            }
        }
        //TH 2:
        //scan grid
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                gridValueDuplicate[x, y] = BoardManager.Instance.gridValue[x, y];
            }
        }

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

            yield return new WaitForSeconds(delayTime);
        }

        //check each moveable block
        for (int i = 0; i < moveableBlocks.Count; i++)
        {
            if (SimulateDrop(moveableBlocks[i]))
            {
                Debug.Log(hint.pos);
                Debug.Log(hint.dir);
                Debug.Log(hint.step);
                //call hint event after
                //use wait until for user select;
                isFoundFullRow = true;
                break;
            }

            yield return new WaitForSeconds(delayTime);
        }

        //moveableBlocks.Clear();
        /*if (!isFoundFullRow)
        {
            Debug.Log("------------------------------------------------");
            for (int x = 0; x < 8; x++)
            {
                for (int y = 9; y > 0; y--)
                {
                    gridValueDuplicate[x, y] = BoardManager.Instance.gridValue[x, y - 1];
                }
            }

            for (int x = 0; x < 8; x++)
            {
                gridValueDuplicate[x, 0] = BoardManager.Instance.standbyRowValue[x];
            }

            string output = "";
            for (int y = 9; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    output += gridValueDuplicate[x, y] + " ";
                }

                output += Environment.NewLine;
            }

            Debug.Log(output);

            for (int i = 0; i < moveableBlocks.Count; i++)
            {
                moveableBlocks[i].IncreaseYPos();
            }

            
            //check each moveable block
            for (int i = 0; i < moveableBlocks.Count; i++)
            {
                if (SimulateDrop(moveableBlocks[i]))
                {
                    Debug.Log(hint.pos);
                    Debug.Log(hint.dir);
                    Debug.Log(hint.step);
                    //call hint event after
                    //use wait until for user select;

                    break;
                }

                yield return new WaitForSeconds(delayTime);
            }

            moveableBlocks.Clear();
        }*/

        yield return null;
    }

    private bool SimulateDrop(MoveableBlock block)
    {
        isFoundFullRow = false;
        bool hasHint = false;


        Vector2 oldPos = block.pos;

        //check left blank
        if (block.leftBlank > 0)
        {
            for (int step = 1; step <= block.leftBlank; step++)
            {
                int[,] grid = new int[8, 10];
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        grid[x, y] = gridValueDuplicate[x, y];
                    }
                }

                Debug.Log("block pos " + "[" + oldPos.x + "," + oldPos.y + "] move left " + step + " steps");
                for (int i = 0; i < block.blockLength; i++)
                {
                    grid[(int) oldPos.x + i, (int) oldPos.y] = 0;
                }

                for (int i = 1; i <= block.blockLength; i++)
                {
                    grid[(int) oldPos.x - step + i - 1, (int) oldPos.y] = block.blockLength * 10 + i;
                }

                ScanMoveDown(grid, true);
                if (isFoundFullRow)
                {
                    hint.dir = "left";
                    hint.pos = new Vector2((int) oldPos.x, (int) oldPos.y);
                    hint.step = step;
                    hasHint = true;
                    break;
                }
                else
                {
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 9; y > 0; y--)
                        {
                            grid[x, y] = grid[x, y - 1];
                        }
                    }

                    for (int x = 0; x < 8; x++)
                    {
                        grid[x, 0] = BoardManager.Instance.standbyRowValue[x];
                    }
                    ScanMoveDown(grid, true);
                    if (isFoundFullRow)
                    {
                        hint.dir = "left";
                        hint.pos = new Vector2((int) oldPos.x, (int) oldPos.y+1);
                        hint.step = step;
                        hasHint = true;
                        break;
                    }
                }
            }
        }

        //if left blanks no answer -> right blanks
        if (!hasHint)
        {
            if (block.rightBlank > 0)
            {
                for (int step = 1; step <= block.rightBlank; step++)
                {
                    int[,] grid = new int[8, 10];
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 10; y++)
                        {
                            grid[x, y] = gridValueDuplicate[x, y];
                        }
                    }

                   Debug.Log("block pos " + "[" + oldPos.x + "," + oldPos.y + "] move right " + step + " steps");
                    for (int i = 0; i < block.blockLength; i++)
                    {
                        grid[(int) oldPos.x + i, (int) oldPos.y] = 0;
                    }

                    for (int i = 1; i <= block.blockLength; i++)
                    {
                        grid[(int) oldPos.x + step + i - 1, (int) oldPos.y] = block.blockLength * 10 + i;
                    }

                    ScanMoveDown(grid, true);
                    if (isFoundFullRow)
                    {
                        hint.dir = "right";
                        hint.pos = new Vector2((int) oldPos.x, (int) oldPos.y);
                        hint.step = step;
                        hasHint = true;
                        break;
                    }
                    else
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 9; y > 0; y--)
                            {
                                grid[x, y] = grid[x, y - 1];
                            }
                        }

                        for (int x = 0; x < 8; x++)
                        {
                            grid[x, 0] = BoardManager.Instance.standbyRowValue[x];
                        }
                        ScanMoveDown(grid, true);
                        if (isFoundFullRow)
                        {
                            hint.dir = "left";
                            hint.pos = new Vector2((int) oldPos.x, (int) oldPos.y+1);
                            hint.step = step;
                            hasHint = true;
                            break;
                        }
                    }
                }
            }
        }

        return hasHint;
    }

    private void ScanMoveDown(int[,] gridValue, bool isContinue)
    {
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
                                isContinue = true;
                                MoveDown(x, y, 1);
                            }

                            break;
                        }
                        case 21:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0)
                            {
                                isContinue = true;
                                MoveDown(x, y, 2);
                            }

                            break;
                        }
                        case 31:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0 &&
                                gridValue[x + 2, y - 1] == 0)
                            {
                                isContinue = true;
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
                                isContinue = true;
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
            string output = "";
            for (int y = 9; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    output += gridValue[x, y] + " ";
                }

                output += Environment.NewLine;
            }

            Debug.Log(output);
            //check full row
            for (int y = 0; y < 10; y++)
            {
                bool isRowFull = true;
                for (int x = 0; x < 8; x++)
                {
                    if (gridValue[x, y] == 0)
                    {
                        isRowFull = false;
                    }
                }

                if (isRowFull)
                {
                    Debug.Log("found");
                    isFoundFullRow = true;
                }
            }
        }


        void MoveDown(int x, int y, int length)
        {
            for (int i = 1; i <= length; i++)
            {
                gridValue[x + i - 1, y] = 0;
                gridValue[x + i - 1, y - 1] = length * 10 + i;
            }
        }
    }
}