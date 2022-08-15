using System;
using AgentDebugTool.Scripts.Utilities;
using UnityEngine;

namespace AgentDebugTool.Scripts
{
    public class TimeScaleUtility : Singleton<TimeScaleUtility>
    {
        private const float TIMESCALE_UPPER_LIMIT = 100f;
        public static event Action<TimeScaleUtility, float> OnTimescaleChanged;
        
        [SerializeField]
        private float timeScaleStep = 1f;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                SetTimeScale(Mathf.Min(Time.timeScale + timeScaleStep, TIMESCALE_UPPER_LIMIT));
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                SetTimeScale(Mathf.Max(Time.timeScale - timeScaleStep, 0f));
            }
        }
        
        public void SetTimeScale(float timeScaleToSet)
        {
            if (Time.timeScale != timeScaleToSet)
            {
                Time.timeScale = timeScaleToSet;
                OnTimescaleChanged?.Invoke(this, Time.timeScale);
            }
        }
    }
}