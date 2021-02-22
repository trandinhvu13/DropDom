using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

    [SerializeField] private GameObject[,] gridGameObjects = new GameObject[8,10];
    [SerializeField] private GameObject grid;

    #endregion

    #region Mono

    #endregion

    #region Methods

    private void SetUpBoard()
    {
        foreach (Transform Child in grid.transform)
        {
            GameObject o;
            Vector2 tempPos = (o = Child.gameObject).GetComponent<Block>().pos;
            gridGameObjects[(int) tempPos.x, (int) tempPos.y] = o;
        }
        Debug.Log(gridGameObjects[7,6].GetComponent<Block>().pos);
    }

    #endregion
}