using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
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
    public bool hasRainbowBlock = false;
    public int blockHasExplodedNum = 0;
    [SerializeField] private float rainbowRandomRate;

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

        return lineValue;
    }

    private void SpawnNewRow() //spawn row below the board
    {
        int[] newRow = GenerateRow();

        for (int i = 0; i < newRow.Length; i++)
        {
            standbyRowValue[i] = newRow[i];

            int blockType = newRow[i] / 10;
            int posInBlock = newRow[i] % 10;
            if (posInBlock == 1)
            {
                bool spawnRainbow = RandomBool();
                GameEvents.Instance.SpawnNewBlock(i, blockType, spawnRainbow);
            }

            /*{
                bool spawnRainbow = RandomBool();
                GameEvents.Instance.SpawnNewBlock(i, 1, spawnRainbow);
            }
            else if (newRow[i] == 21)
            {
                bool spawnRainbow = RandomBool();
                GameEvents.Instance.SpawnNewBlock(i, 2, spawnRainbow);
            }
            else if (newRow[i] == 31)
            {
                bool spawnRainbow = RandomBool();
                GameEvents.Instance.SpawnNewBlock(i, 3, spawnRainbow);
            }
            else if (newRow[i] == 41)
            {
                bool spawnRainbow = RandomBool();
                GameEvents.Instance.SpawnNewBlock(i, 4, spawnRainbow);
            }*/
        }

        bool RandomBool()
        {
            float ran = Random.value;
            if (ran < rainbowRandomRate)
            {
                if (!hasRainbowBlock)
                {
                    Debug.Log(ran);
                    hasRainbowBlock = true;
                    return true;
                }
            }

            return false;
        }
    }

    public void MoveUpNewRow() //move new row from under the board up on the board
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
        //yield animation
        ScanMoveDown(true);
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

    public void ScanMoveDown(bool isContinue)
    {
        if (isContinue)
        {
            isContinue = false;
            bool isFound = false;
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
                                isFound = true;
                            }

                            break;
                        }
                        case 21:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0)
                            {
                                MoveDown(x, y, 2);
                                isFound = true;
                            }

                            break;
                        }
                        case 31:
                        {
                            if (gridValue[x, y - 1] == 0 && gridValue[x + 1, y - 1] == 0 &&
                                gridValue[x + 2, y - 1] == 0)
                            {
                                MoveDown(x, y, 3);
                                isFound = true;
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
                                isFound = true;
                            }

                            break;
                        }
                    }
                }
            }

            if (isFound)
            {
                numOfScan++;
            }

            ScanMoveDown(isContinue);
        }
        else
        {
            // done scan + execute move down (send message to block)
            //reset num of step array
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (numOfStepDown[x, y] != 0)
                    {
                        GameEvents.Instance.BlockMoveDown(new Vector2(x, y), numOfStepDown[x, y]);
                    }

                    numOfStepDown[x, y] = 0;
                }
            }

            numOfScan = 0;
            blockHasExplodedNum = 0;
            ScanForFullRow();
            GameEvents.Instance.FindLimitArea();
        }

        //move down logically
        void MoveDown(int x, int y, int length)
        {
            isContinue = true;
            for (int i = 1; i <= length; i++)
            {
                gridValue[x + i - 1, y] = 0;
                gridValue[x + i - 1, y - 1] = length * 10 + i;
            }

            if (numOfStepDown[x, y + numOfScan] != 0)
            {
                numOfStepDown[x, y + numOfScan]++;
            }
            else
            {
                numOfStepDown[x, y]++;
            }

            // numOfScan++;
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
        for (int i = 0; i < length; i++)
        {
            gridValue[(int) oldPos.x + i, (int) oldPos.y] = 0;
        }

        for (int i = 1; i <= length; i++)
        {
            gridValue[(int) newPos.x + i - 1, (int) newPos.y] = length * 10 + i;
        }
    }

    public void ScanForFullRow()
    {
        for (int y = 0; y < 10; y++)
        {
            bool isFoundFullRow = true;
            for (int x = 0; x < 8; x++)
            {
                if (gridValue[x, y] == 0)
                {
                    isFoundFullRow = false;
                }
            }

            StartCoroutine(Explode(isFoundFullRow, y));
        }

        IEnumerator Explode(bool _isFoundFullRow, int _y)
        {
            if (_isFoundFullRow)
            {
                int numOfBlockInRow = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (gridValue[i, _y] == 11 || gridValue[i, _y] == 21 || gridValue[i, _y] == 31 ||
                        gridValue[i, _y] == 41)
                    {
                        numOfBlockInRow++;
                    }

                    gridValue[i, _y] = 0;
                    GameEvents.Instance.BlockExplode(new Vector2(i, _y));
                }

                Debug.Log("block in rainbow row " + numOfBlockInRow);
                while (blockHasExplodedNum < numOfBlockInRow)
                {
                    Debug.Log("wait");
                    yield return null;
                }

                yield return new WaitForSeconds(1);
                Debug.Log("has explode " + blockHasExplodedNum);
                ScanMoveDown(true); // after an amount of time
            }

            yield return null;
        }
    }

    public List<Vector2> GetNearbyBlocks(Vector2 pos, int blockLength)
    {
        List<Vector2> nearbyList = new List<Vector2>();
        for (int i = 0; i < blockLength; i++)
        {
            //check block above
            if ((int) pos.y < 9)
            {
                int value = gridValue[(int) pos.x + i, (int) pos.y + 1];
                if (value != 0 && value % 10 != 1)
                {
                    Vector2 originPosOfBlock = new Vector2((int) pos.x + i - (value % 10) + 1, (int) pos.y + 1);
                    if (!nearbyList.Contains(originPosOfBlock))
                    {
                        nearbyList.Add(originPosOfBlock);
                    }
                }
                else if (value % 10 == 1)
                {
                    Vector2 originPosOfBlock = new Vector2((int) pos.x + i, (int) pos.y + 1);
                    if (!nearbyList.Contains(originPosOfBlock))
                    {
                        nearbyList.Add(originPosOfBlock);
                    }
                }
            }

            //check block below
            if ((int) pos.y > 0)
            {
                int value = gridValue[(int) pos.x + i, (int) pos.y - 1];
                if (value != 0 && value % 10 != 1)
                {
                    Vector2 originPosOfBlock = new Vector2((int) pos.x + i - (value % 10) + 1, (int) pos.y - 1);
                    if (!nearbyList.Contains(originPosOfBlock))
                    {
                        nearbyList.Add(originPosOfBlock);
                    }
                }
                else if (value % 10 == 1)
                {
                    Vector2 originPosOfBlock = new Vector2((int) pos.x + i, (int) pos.y - 1);
                    if (!nearbyList.Contains(originPosOfBlock))
                    {
                        nearbyList.Add(originPosOfBlock);
                    }
                }
            }
        }

        return nearbyList;
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
        for (int y = 9; y >= 0; y--)
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