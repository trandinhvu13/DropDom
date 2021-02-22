using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager _instance;

        public static GameManager Instance { get { return _instance; } }

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
        }
        #endregion

        #region Variables

        

        #endregion

        #region Mono

        

        #endregion

        #region Methods

        

        #endregion
}
