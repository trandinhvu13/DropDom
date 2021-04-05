using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

        }
    
        #endregion

        #region Variables

        [Header("Tween")] 
        public float moveDownTime;
        public LeanTweenType moveDownTween;
        public float moveUpTime;
        public LeanTweenType moveUpTween;
        public float moveToTileTime;
        public LeanTweenType moveToTileTween;
        public float explodeTime;
        public LeanTweenType explodeTween;
        public float rainbowExplodeTime;

        #endregion
}
