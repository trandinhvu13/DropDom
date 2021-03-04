using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    #region Singleton
    private static GameEvents _instance;

    public static GameEvents Instance { get { return _instance; } }


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
    }
    #endregion

    #region Tile
    public event Action<int, int> OnSpawnNewBlock; 
    public void SpawnNewBlock(int pos, int blockType)
    {
        OnSpawnNewBlock?.Invoke(pos, blockType);
    }
    

    #endregion

    #region Block
    public event Action OnBlockMoveUp; 
    public void BlockMoveUp()
    {
        OnBlockMoveUp?.Invoke();
    }
    
    public event Action<Vector2, int> OnBlockMoveDown; 
    public void BlockMoveDown(Vector2 pos, int step)
    {
        OnBlockMoveDown?.Invoke(pos, step);
    }
    public event Action OnFindLimitArea;
    public void FindLimitArea()
    {
        OnFindLimitArea?.Invoke();
    }
    public event Action<Vector2> OnBlockExplode; 
    public void BlockExplode(Vector2 pos)
    {
        OnBlockExplode?.Invoke(pos);
    }

    #endregion
    
}
