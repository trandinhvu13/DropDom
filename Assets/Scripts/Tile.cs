using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using Shapes2D;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Tile : MonoBehaviour
{
    #region Variables

    public Vector2 pos;
    public Vector2 value = Vector2.zero; //x là độ dài block, y là thứ tự trong block
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private bool isStandbyTile;
    public Shape shape2d;
    private bool isHighlighted = false;
    [SerializeField] private Color32 normalColor;
    [SerializeField] private Color32 highlightedColor;

    #endregion

    #region Mono

    private void Awake()
    {
        SetUpPos();
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnSpawnNewBlock += SpawnBlock;
        GameEvents.Instance.OnHighlightBlock += ChangeToHighlightColor;
        GameEvents.Instance.OnDehighlightBlock += ChangeToNormalColor;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnSpawnNewBlock -= SpawnBlock;
        GameEvents.Instance.OnHighlightBlock -= ChangeToHighlightColor;
        GameEvents.Instance.OnDehighlightBlock -= ChangeToNormalColor;
    }

    #endregion

    #region Methods

    private void SetUpPos()
    {
        var localPosition = transform.localPosition;
        pos.x = localPosition.x;
        pos.y = localPosition.y;
        if (!isStandbyTile)
        {
            gameObject.name = "[" + pos.x + "," + pos.y + "]";
        }
    }

    private void SpawnBlock(Vector2 _pos, int _blockType, bool _isRainbow)
    {
        if (_pos == pos)
        {
            int yPos = (int) transform.position.y;
            GameObject prefabToSpawn = blockPrefabs[_blockType - 1];
            float spawnOffset = 0;

            if (_blockType == 1)
            {
                spawnOffset = 0;
            }
            else if (_blockType == 2)
            {
                spawnOffset = 0.5f;
            }
            else if (_blockType == 3)
            {
                spawnOffset = 1;
            }
            else if (_blockType == 4)
            {
                spawnOffset = 1.5f;
            }

            Vector3 spawnPos = new Vector3(transform.position.x + spawnOffset, yPos, -2);

            GameObject block = LeanPool.Spawn(prefabToSpawn, spawnPos, Quaternion.identity, transform);

            var blockScript = block.GetComponent<Block>();

            blockScript.pos = new Vector2(pos.x, pos.y);
            blockScript.isRainbow = _isRainbow;
            if (pos.y > -1)
            {
                blockScript.isOnBoard = true;
            }
        }
    }

    public void ChangeToHighlightColor(int x)
    {
        if (pos.y != -1)
        {
            if ((int) pos.x != x) return;
            if (!isHighlighted)
            {
                isHighlighted = true;
                shape2d.settings.fillColor = highlightedColor;
            }  
        }
    }

    public void ChangeToNormalColor(int x)
    {
        if (pos.y != -1)
        {
            if ((int) pos.x != x) return;
            if (isHighlighted)
            {
                isHighlighted = false;
                shape2d.settings.fillColor = normalColor;
            }  
        }
    }

    #endregion
}