using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Xml.Schema;
using Shapes2D;
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
    public Transform[,] gridPoses = new Transform[8, 10];
    public Transform[] standbyRowPoses = new Transform[8];
    public int[,] gridValue = new int[8, 10];
    public int[,] numOfStepDown = new int[8, 10];
    public int[] standbyRowValue = new int[8];
    [SerializeField] private GameObject grid;
    [SerializeField] private GameObject standbyRow;
    [SerializeField] private int numOfScan = 0;
    [SerializeField] private float checkDistance;
    public bool hasRainbowBlock = false;
    [SerializeField] private float rainbowRandomRate;
    public bool hasMovedUp = false;
    [SerializeField] private bool isNewGame = true;
    public bool canDrag = false;
    public Vector2 rainbowPos;
    public String[] blockColors;
    public bool canMoveDown = true;
    [SerializeField] private GameObject hintScannerGameObject;
    [SerializeField] private HintScanner hintScannerScript;

    #endregion

    #region Mono

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugWholeArray(gridValue);
        }

/*
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Pressed");
            ScanMoveDown(true);
        }

        if (Input.GetKeyDown((KeyCode.A)))
        {
            StartCoroutine(MoveUpNewRow());
        }

        if (Input.GetKeyDown((KeyCode.D)))
        {
            GameEvents.Instance.FindLimitArea();
        }*/
    }

    #endregion

    #region Methods

    private void SetUpBoard()
    {
        foreach (Transform Child in grid.transform)
        {
            GameObject o;
            Vector2 tempPos = (o = Child.gameObject).GetComponent<Tile>().pos;
            //Child.gameObject.GetComponent<Tile>().shape2d = Child.gameObject.GetComponent<Tile>().GetComponent<Shape>();
            gridGameObjects[(int) tempPos.x, (int) tempPos.y] = o;
            gridPoses[(int) tempPos.x, (int) tempPos.y] = Child.transform;
            
        }

        foreach (Transform Child in standbyRow.transform)
        {
            int x = (int) Child.gameObject.GetComponent<Tile>().pos.x;

            standbyRowGameObjects[x] = Child.gameObject;
            standbyRowPoses[x] = Child.GetComponent<Tile>().transform;
        }

        rainbowPos = GameManager.Instance.savedRainbowPos;
        if (rainbowPos != new Vector2(-5, -5))
        {
            hasRainbowBlock = true;
        }

        if (GameManager.Instance.isNewGame)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    gridValue[x, y] = 0; //0 = blank
                }
            }

            //set up cho tutorial
            SpawnNewRow(null, 0);
            SpawnNewRow(null, 1);
            SpawnNewRow();
        }
        else
        {
            gridValue = GameManager.Instance.savedGridValue;
            standbyRowValue = GameManager.Instance.savedStandbyRow;
            for (int y = 0; y < 10; y++)
            {
                int[] row = new int[8];
                for (int x = 0; x < 8; x++)
                {
                    row[x] = gridValue[x, y];
                }

                SpawnNewRow(row, y);
            }

            SpawnNewRow(standbyRowValue, -1);


            GameEvents.Instance.ChangeToRainbow(rainbowPos);
        }

        StartCoroutine(NewGameAction());
        GameEvents.Instance.ToggleHintScanner(true);
        //start new game


        IEnumerator NewGameAction()
        {
            yield return new WaitForSeconds(0.2f);
            ScanMoveDown(true);
            GameEvents.Instance.FindLimitArea();
            GameManager.Instance.isNewGame = false;
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

    private void SpawnNewRow(int[] row = null, int y = -1) //spawn row below the board
    {
        int[] newRow;
        if (row == null)
        {
            newRow = GenerateRow();
        }
        else
        {
            newRow = row;
        }


        for (int i = 0; i < newRow.Length; i++)
        {
            if (y == -1)
            {
                standbyRowValue[i] = newRow[i];
            }
            else
            {
                gridValue[i, y] = newRow[i];
            }

            int blockType = newRow[i] / 10;
            int posInBlock = newRow[i] % 10;
            if (posInBlock == 1)
            {
                bool spawnRainbow = RandomBool();
                if (spawnRainbow)
                {
                    rainbowPos = new Vector2(i, y);
                }

                GameEvents.Instance.SpawnNewBlock(new Vector2(i, y), blockType, spawnRainbow);
            }
        }

        bool RandomBool()
        {
            if (y == -1)
            {
                float ran = Random.value;
                if (ran < rainbowRandomRate)
                {
                    if (!hasRainbowBlock)
                    {
                        hasRainbowBlock = true;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    IEnumerator MoveUpNewRow() //move new row from under the board up on the board
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
        yield return new WaitForSeconds(AnimationManager.Instance.moveUpTime);
        ScanMoveDown(true);
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
        if (!canMoveDown)
        {
            return;
        }

        if (isContinue)
        {
            isContinue = false;
            bool isFound = false;
            for (int y = 1; y < 10; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int blockLength = gridValue[x, y] / 10;
                    if (gridValue[x, y] % 10 == 1)
                    {
                        bool canDrop = true;
                        for (int i = 0; i < blockLength; i++)
                        {
                            if (gridValue[x + i, y - 1] != 0)
                            {
                                canDrop = false;
                                break;
                            }
                        }

                        if (canDrop)
                        {
                            MoveDown(x, y, blockLength);
                            isFound = true;
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
            //GameEvents.Instance.FindNearbyBlocks();
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

            StartCoroutine(ScanForFullRow());
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

    IEnumerator ScanForFullRow()
    {
        yield return new WaitForSeconds(AnimationManager.Instance.moveDownTime);
        bool isFoundFullRow = false;
        List<int> fullRows = new List<int>();

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
                fullRows.Add(y);
                isFoundFullRow = true;
            }
        }

        for (int i = 0; i < fullRows.Count; i++)
        {
            if (fullRows[i] == rainbowPos.y)
            {
                fullRows.Clear();
                fullRows.Add((int) rainbowPos.y);
            }
        }

        if (isFoundFullRow)
        {
            for (int i = 0; i < fullRows.Count; i++)
            {
                StartCoroutine(Explode(fullRows[i]));
            }
        }
        else
        {
            if (hasMovedUp == false)
            {
                hasMovedUp = true;
                if (isNewGame)
                {
                    isNewGame = false;
                    canDrag = true;
                    yield return null;
                }
                else
                {
                    StartCoroutine(MoveUpNewRow());
                }
            }
            else
            {
                if (IsBoardEmpty())
                {
                    StartCoroutine(MoveUpNewRow());
                }

                canDrag = true;
                //GameEvents.Instance.ToggleHintScanner(true);
            }
        }

        // them arg
        IEnumerator Explode(int _y)
        {
            bool hasFullRowRainbow = false;

            if (hasRainbowBlock && _y == rainbowPos.y)
            {
                hasFullRowRainbow = true;
                List<Vector2> nearbyBlocks = GetNearbyBlocks(rainbowPos, (int) gridValue[(int) rainbowPos.x,
                    (int) rainbowPos.y] / 10);

                for (int i = 0; i < nearbyBlocks.Count; i++)
                {
//                    Debug.Log(nearbyBlocks[i]);
                    GameEvents.Instance.RainbowBlockAnimation(nearbyBlocks[i]);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (gridValue[i, (int) rainbowPos.y] == 11 || gridValue[i, (int) rainbowPos.y] == 21 ||
                        gridValue[i, (int) rainbowPos.y] == 31 || gridValue[i, (int) rainbowPos.y] == 41)
                    {
                        GameEvents.Instance.RainbowBlockAnimation(new Vector2(i, rainbowPos.y));
                    }
                }
            }

            for (int i = 0; i < 8; i++)
            {
                gridValue[i, _y] = 0;
                GameEvents.Instance.BlockExplode(new Vector2(i, _y), hasFullRowRainbow);
            }

            if (hasFullRowRainbow)
            {
                canMoveDown = false;
                yield return new WaitForSeconds(AnimationManager.Instance.rainbowExplodeTime);
                canMoveDown = true;
            }

            yield return new WaitForSeconds(AnimationManager.Instance.explodeTime);

            ScanMoveDown(true); // after an amount of time

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

    public void DeleteBlock(Vector2 pos)
    {
        int value = gridValue[(int) pos.x, (int) pos.y];
        int length = (int) value / 10;

        for (int i = 0; i < length; i++)
        {
            gridValue[(int) pos.x + i, (int) pos.y] = 0;
        }
    }

    public bool IsBoardEmpty()
    {
        for (int x = 1; x < 8; x++)
        {
            if (gridValue[x, 0] != 0)
            {
                return false;
            }
        }

        return true;
    }

    /*private void DebugLogArray(int[] array)
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
*/
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