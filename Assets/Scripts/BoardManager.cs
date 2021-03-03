using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    #region Singleton

    private static BoardManager _instance;

    public static BoardManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        SetUpBoard();
    }

    #endregion

    #region Variables

    public GameObject[,] gridGameObjects = new GameObject[8, 10];
    private GameObject[] standbyRowGameObjects = new GameObject[8];
    public int[,] gridValue = new int[8, 10];
    public int[,] numOfStepDown = new int[8, 10];
    public int[] standbyRowValue = new int[8];
    [SerializeField] private GameObject grid;
    [SerializeField] private GameObject standbyRow;
    [SerializeField] private int numOfScan = 0;
    [SerializeField] private float checkDistance;

    #endregion

    #region Mono

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugWholeArray(gridValue);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            DebugWholeArray(numOfStepDown);
        }

        if (Input.GetKeyDown((KeyCode.A)))
        {
            MoveUpNewRow();
        }
    }

    #endregion

    #region Methods

    private void SetUpBoard()
    {
        foreach (Transform Child in grid.transform)
        {
            GameObject o;
            Vector2 tempPos = (o = Child.gameObject).GetComponent<Tile>().pos;
            gridGameObjects[(int) tempPos.x, (int) tempPos.y] = o;
        }

        foreach (Transform Child in standbyRow.transform)
        {
            int x = (int) Child.gameObject.GetComponent<Tile>().pos.x;

            standbyRowGameObjects[x] = Child.gameObject;
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                gridValue[x, y] = 0; //0 = blank
            }
        }
    }

    private int[] GenerateRow()
    {
        int[] lineValue = {-1, -1, -1, -1, -1, -1, -1, -1}; //array represents upcoming line


        int blankPos = Random.Range(1, 8 + 1); //position of the blank
        int blankBlockLength = Random.Range(1, 8 - blankPos + 1); //length of that blank

        if (blankBlockLength > 3)
        {
            blankBlockLength = 3;
        }

        var numOfUnassignedTiles = 8 - blankBlockLength;

        for (int i = 0; i < blankBlockLength; i++)
        {
            lineValue[blankPos - 1 + i] = 0;
        }

        int numOfBlocksInARow =
            Random.Range(2, 8 - blankBlockLength + 1); //number of separate blocks in a row (exclude blank)
        if (numOfBlocksInARow > 4)
        {
            numOfBlocksInARow = 4;
        }

        lineValue[blankPos - 1] = 0; //0 = blank, -1 = unassigned value
        if (blankPos == 1 || blankPos == 8)
        {
            GenerateBlock(numOfUnassignedTiles, numOfBlocksInARow);
        }
        else
        {
            int numOfBlocksInARow1;

            if (blankPos - 1 > 4)
            {
                numOfBlocksInARow1 = Random.Range(2, numOfBlocksInARow);
            }
            else
            {
                numOfBlocksInARow1 = Random.Range(1, blankPos - 1 + 1);
                while (numOfBlocksInARow1 > numOfBlocksInARow)
                {
                    numOfBlocksInARow1 = Random.Range(1, blankPos - 1 + 1);
                }
            }


            GenerateBlock(blankPos - 1, numOfBlocksInARow1); //generate for the first blank part
            int numOfRemainingTiles = 8 - (blankPos + blankBlockLength - 1);

            int numOfBlocksInARow2;

            numOfBlocksInARow2 =
                Random.Range(numOfRemainingTiles > 4 ? 2 : 1, numOfBlocksInARow - numOfBlocksInARow1 + 1);
            GenerateBlock(numOfRemainingTiles, numOfBlocksInARow2); //generate for the 2nd blank part
        }


        void GenerateBlock(int _unassignedTileNum, int _blocksInARowNum)
        {
            int unassignedTileNum = _unassignedTileNum;
            int blocksInARowNum = _blocksInARowNum;
            int count = _blocksInARowNum; //count in for loop
            for (int i = 0; i < count; i++)
            {
                int tempBlockLength;
                if (unassignedTileNum - blocksInARowNum == 0)
                {
                    tempBlockLength = 1;
                }
                else if (blocksInARowNum > 1)
                {
                    tempBlockLength = Random.Range(1, unassignedTileNum);
                    while (tempBlockLength >= 5 || (unassignedTileNum - tempBlockLength > 4))
                    {
                        tempBlockLength = Random.Range(1, unassignedTileNum + 1);
                    }
                }
                else
                {
                    tempBlockLength = unassignedTileNum;
                }

                for (int j = 1; j <= tempBlockLength; j++) //add a block to linevalue[]
                {
                    AddToLine(tempBlockLength * 10 + j);
                }

                unassignedTileNum -= tempBlockLength;
                blocksInARowNum--;
            }
        }

        void AddToLine(int _value)
        {
            for (int i = 0; i < lineValue.Length; i++)
            {
                if (lineValue[i] == -1)
                {
                    lineValue[i] = _value;
                    break;
                }
            }
        }

        //DebugLogArray(lineValue);
        return lineValue;
    }

    private void SpawnNewRow() //spawn row below the board
    {
        int[] newRow = GenerateRow();

        for (int i = 0; i < newRow.Length; i++)
        {
            standbyRowValue[i] = newRow[i];
            if (newRow[i] == 11)
            {
                GameEvents.Instance.SpawnNewBlock(i, 1);
            }
            else if (newRow[i] == 21)
            {
                GameEvents.Instance.SpawnNewBlock(i, 2);
            }
            else if (newRow[i] == 31)
            {
                GameEvents.Instance.SpawnNewBlock(i, 3);
            }
            else if (newRow[i] == 41)
            {
                GameEvents.Instance.SpawnNewBlock(i, 4);
            }
        }
    }

    private void MoveUpNewRow() //move new row from under the board up on the board
    {
        for (int x = 0; x < 8; x++) //change gridvalue array
        {
            for (int y = 9; y >= 0; y--)
            {
                if (y > 0)
                {
                    gridValue[x, y] = gridValue[x, y - 1];
                }
                else
                {
                    gridValue[x, y] = standbyRowValue[x];
                }
            }
        }

        GameEvents.Instance.BlockMoveUp();
        SpawnNewRow();
    }

    public int ReturnBlankLength(int x, int y, int blockLength, string dir)
    {
        int blankLength = 0;
        if ((x == 0 && dir == "left") || (x == 7 && dir == "right"))
        {
            return 0;
        }

        if (dir == "left")
        {
            if (x == 0)
            {
                return 0;
            }

            if (gridValue[x - 1, y] != 0)
            {
                return 0;
            }

            for (int i = 0; i < 7; i++)
            {
                if (x - 1 - i >= 0)
                {
                    if (gridValue[x - 1 - i, y] == 0)
                    {
                        blankLength++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return blankLength;
        }

        if (dir == "right")
        {
            if (x + blockLength - 1 >= 7)
            {
                return 0;
            }

            if (gridValue[x + blockLength, y] != 0)
            {
                return 0;
            }

            for (int i = 0; i < 7; i++)
            {
                if (x + blockLength + i <= 7)
                {
                    if (gridValue[x + blockLength + i, y] == 0)
                    {
                        blankLength++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return blankLength;
        }

        return 0;
    }

    public void ScanMoveDownArrayValue(bool isContinue)
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

            ScanMoveDownArrayValue(isContinue);
        }
        else
        {
            // done scan + execute move down (send message to block)

            //reset num of step array
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    numOfStepDown[x, y] = 0;
                }
            }

            numOfScan = 0;
            Debug.Log("End Scan");
        }

        void MoveDown(int x, int y, int length)
        {
            isContinue = true;
            numOfStepDown[x, y + numOfScan]++;
            for (int i = 1; i <= length; i++)
            {
                gridValue[x + i - 1, y] = 0;
                gridValue[x + i - 1, y - 1] = length * 10 + i;
            }

            numOfScan++;
        }
    }

    public GameObject CheckDropPos(Transform pos, int y)
    {
        float shortestDistance = checkDistance;
        GameObject matchedTile = null;
        for (int i = 0; i < 8; i++)
        {
            var tempDistance = Vector2.Distance(pos.position, gridGameObjects[i, y].transform.position);
            if (tempDistance < shortestDistance)
            {
                shortestDistance = tempDistance;
                matchedTile = gridGameObjects[i, y];
            }
        }

        return matchedTile;
    }

    public void DragBlockFingerUp(Vector2 oldPos, Vector2 newPos, int length)
    {
    }

    private void DebugLogArray(int[] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
        {
            output += array[i] + " ";
        }

        Debug.Log(output);
    }

    private void DebugALine(int y)
    {
        string output = "Line " + y + ": ";
        for (int i = 0; i < 8; i++)
        {
            output += gridValue[i, y] + " ";
        }

        Debug.Log(output);
    }

    private void DebugWholeArray(int[,] array)
    {
        string output = "";
        for (int y = 7; y >= 0; y--)
        {
            for (int x = 0; x < 8; x++)
            {
                output += array[x, y] + " ";
            }

            output += Environment.NewLine;
        }

        Debug.Log(output);
    }

    #endregion
}