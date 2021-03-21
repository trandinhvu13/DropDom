using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeightedRandomization;


public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager _instance;

    public static GameManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        LoadGameData();
    }

    #endregion

    #region Variables

    public float updateInterval;
    public bool isNewGame;
    public int[] savedStandbyRow = new int[8];
    public int[,] savedGridValue = new int[8, 10];
    public Vector2 savedRainbowPos = new Vector2(-5,-5);

    #endregion

    #region Randomize Value

    public WeightedRandomizer<int> blockRand = new WeightedRandomizer<int>();

    #endregion

    #region Mono

    private void OnApplicationQuit()
    {
        SaveCurrentGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentGame();
        }
    }

    #endregion

    #region Save Game

    public void SaveCurrentGame()
    {
        //setting audio vv
        PlayerPrefsX.SetBool("isNewGame", isNewGame);
        //rainbow pos
//        Debug.Log(BoardManager.Instance.rainbowPos);
        PlayerPrefsX.SetVector2("rainbowPos", BoardManager.Instance.rainbowPos);
        //array stanby row
        PlayerPrefsX.SetIntArray("standbyRow", BoardManager.Instance.standbyRowValue);
        //other line
        for (int y = 0; y < 10; y++)
        {
            int[] row = new int[8];
            for (int x = 0; x < 8; x++)
            {
                row[x] = BoardManager.Instance.gridValue[x, y];
            }

            string rowName = "row" + y;
            PlayerPrefsX.SetIntArray(rowName, row);
        }
    }

    public void LoadGameData()
    {
        //check if new game
        isNewGame = PlayerPrefsX.GetBool("isNewGame", true);
        //take data fromm playerpref
        if (isNewGame)
        {
            return;
        }

        savedRainbowPos = PlayerPrefsX.GetVector2("rainbowPos", new Vector2(-5, -5));
        int[] standByRow = PlayerPrefsX.GetIntArray("standbyRow");
        for (int x = 0; x < 8; x++)
        {
            savedStandbyRow[x] = standByRow[x];
        }

        for (int y = 0; y < 10; y++)
        {
            int[] row = new int[8];
            string rowName = "row" + y;
            row = PlayerPrefsX.GetIntArray(rowName);
            for (int x = 0; x < 8; x++)
            {
                savedGridValue[x, y] = row[x];
            }
        }
    }

    #endregion

    #region Methods

    #endregion
}