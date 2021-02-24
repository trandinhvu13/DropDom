using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private GameObject[,] gridGameObjects = new GameObject[8, 10];
    [SerializeField] private GameObject grid;

    #endregion

    #region Mono

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateRow();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            int test = Random.Range(1, 5);
            Debug.Log(test);
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
    }

    private void GenerateRow()
    {
        //int blankNum = Random.Range(1, 3); //how many blank in a row
        int blankNum = 1;

        int[] lineValue = {-1, -1, -1, -1, -1, -1, -1, -1}; //array represents upcoming line

        if (blankNum == 1)
        {
            int blankPos = Random.Range(1, 8 + 1); //position of the blank
            //int blankPos = 2;
            int blankBlockLength = Random.Range(1, 8 - blankPos + 1); //length of that blank

            Debug.Log("Blank Pos: " + blankPos);

            if (blankBlockLength > 3)
            {
                blankBlockLength = 3;
            }

            var numOfUnassignedTiles = 8 - blankBlockLength;

            for (int i = 0; i < blankBlockLength; i++)
            {
                lineValue[blankPos - 1 + i] = 0;
            }

            Debug.Log("Blank Length: " + blankBlockLength);
            DebugLogArray(lineValue);

            int numOfBlocksInARow =
                Random.Range(2, 8 - blankBlockLength + 1); //number of separate blocks in a row (exclude blank)
            if (numOfBlocksInARow > 4)
            {
                Debug.Log("Again: Unassigned: " + numOfUnassignedTiles + "| num of block " + numOfBlocksInARow);
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
               // int numOfBlocksInARow1 = Random.Range(1, blankPos - 1 + 1);
                if (blankPos - 1 > 4)
                {
                   numOfBlocksInARow1 = Random.Range(2, blankPos - 1 + 1);
                   while (numOfBlocksInARow1 > numOfBlocksInARow)
                   {
                       numOfBlocksInARow1 = Random.Range(2, blankPos - 1 + 1);
                   }
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

                Debug.Log("Second part---------------------------------");
                int numOfRemainingTiles = 8 - (blankPos + blankBlockLength - 1);

                int numOfBlocksInARow2;
                if (numOfRemainingTiles > 4)
                {
                    numOfBlocksInARow2 = Random.Range(2, numOfBlocksInARow - numOfBlocksInARow1 + 1);
                }
                else
                {
                    numOfBlocksInARow2 = Random.Range(1, numOfBlocksInARow - numOfBlocksInARow1 + 1);
                }
                // while (numOfRemainingTiles/numOfBlocksInARow2 > 4)
                // {
                //     numOfBlocksInARow2 = Random.Range(1, numOfBlocksInARow - numOfBlocksInARow1 + 1);
                // }

                GenerateBlock(numOfRemainingTiles, numOfBlocksInARow2); //generate for the 2nd blank part
            }
        }
        else //rare
        {
        }

        void GenerateBlock(int _unassignedTileNum, int _blocksInARowNum)
        {
            int unassignedTileNum = _unassignedTileNum;
            int blocksInARowNum = _blocksInARowNum;

            int count = _blocksInARowNum; //count in for loop
            for (int i = 0; i < count; i++)
            {
                Debug.Log("Unassigned " + unassignedTileNum);
                Debug.Log("Block in a row " + blocksInARowNum);
                int tempBlockLength;
                if (i != count - 1 && blocksInARowNum != 1) 
                {
                    tempBlockLength = Random.Range(1, unassignedTileNum + 1);
                    while (tempBlockLength >= 5 || (unassignedTileNum - tempBlockLength > 4))
                    {
                        tempBlockLength = Random.Range(1, unassignedTileNum + 1);
                    }
                }
                else
                {
                    tempBlockLength = unassignedTileNum;
                }

                Debug.Log("Temp Block: " + tempBlockLength);
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
                    Debug.Log("Add " + _value + " at " + i);
                    DebugLogArray(lineValue);
                    break;
                }
            }
        }

        DebugLogArray(lineValue);
        Debug.Log("END---------------------------------------------------------------------");
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

    #endregion
}