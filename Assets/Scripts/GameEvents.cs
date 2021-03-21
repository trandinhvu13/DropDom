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
    public event Action<Vector2, int, bool> OnSpawnNewBlock; 
    public void SpawnNewBlock(Vector2 pos, int blockType, bool isRainbow)
    {
        OnSpawnNewBlock?.Invoke(pos, blockType, isRainbow);
    }
    public event Action<int> OnHighlightBlock; 
    public void HighlightBlock(int x)
    {
        OnHighlightBlock?.Invoke(x);
    }
    public event Action<int> OnDehighlightBlock; 
    public void DehighlightBlock(int x)
    {
        OnDehighlightBlock?.Invoke(x);
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
    public event Action OnFindNearByBlocks;
    public void FindNearbyBlocks()
    {
        OnFindNearByBlocks?.Invoke();
    }
    public event Action<Vector2, bool> OnBlockExplode; 
    public void BlockExplode(Vector2 pos, bool hasFullRowRainbow)
    {
        OnBlockExplode?.Invoke(pos, hasFullRowRainbow);
    }
    public event Action<Vector2> OnRainbowBlockAnimation; 
    public void RainbowBlockAnimation(Vector2 pos)
    {
        OnRainbowBlockAnimation?.Invoke(pos);
    }
    public event Action<Vector2> OnChangeToRainbow; 
    public void ChangeToRainbow(Vector2 pos)
    {
        OnChangeToRainbow?.Invoke(pos);
    }
    #endregion
    
}
