using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimationManager : MonoBehaviour
{
    #region Singleton

    private static AnimationManager _instance;

    public static AnimationManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    #endregion

    #region Variables

    [Header("Tween")] public float moveDownTime;
    public LeanTweenType moveDownTween;
    public float moveUpTime;
    public LeanTweenType moveUpTween;
    public float moveToTileTime;
    public LeanTweenType moveToTileTween;
    public float explodeTime;
    public LeanTweenType explodeTween;
    public float rainbowExplodeTime;
    public LeanTweenType fadeBackgroundTween;
    public float fadeBackgroundTime;
    [Header("Pause Menu")] 
    public bool isPause = false;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private SpriteRenderer soundToggle;
    [SerializeField] private Sprite soundOn;
    [SerializeField] private Sprite soundOff;
    [Header("Fade")] [SerializeField] private SpriteRenderer fadeBackground;
    #endregion

    #region UI

    public void Pause()
    {
        GameEvents.Instance.ToggleCollider(false);
        fadeBackground.sortingLayerName = "FadeUI";
        FadeInBackground();
        isPause = true;
        pausePanel.SetActive(true);
        GameEvents.Instance.ChangeCanPress("pause", false);
        GameEvents.Instance.ChangeCanPress("sound", true);
        GameEvents.Instance.ChangeCanPress("xPause", true);
        GameEvents.Instance.ChangeCanPress("replay", true);
    }

    public void XPause()
    {
        GameEvents.Instance.ToggleCollider(true);
        isPause = false;
        FadeOutBackground();
        GameEvents.Instance.ChangeCanPress("pause", true);
        GameEvents.Instance.ChangeCanPress("sound", false);
        GameEvents.Instance.ChangeCanPress("xPause", false);
        GameEvents.Instance.ChangeCanPress("replay", false);
        pausePanel.SetActive(false);
    }

    public void ToggleSound()
    {
    }

    public void Replay()
    {
    }

    public void Retry()
    {
    }

    void FadeInBackground()
    {
        LeanTween.value(gameObject, 0, 90, fadeBackgroundTime).setEase(fadeBackgroundTween).setOnUpdate((val) =>
        {
            fadeBackground.color = new Color32(10, 10, 10, (byte)val);
        });
    }

    void FadeOutBackground()
    {
        LeanTween.value(gameObject, 90, 0, fadeBackgroundTime).setEase(fadeBackgroundTween).setOnUpdate((val) =>
        {
            fadeBackground.color = new Color32(10, 10, 10, (byte)val);
        });
    }
    #endregion
}