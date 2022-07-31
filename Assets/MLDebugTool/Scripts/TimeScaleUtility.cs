using System;
using MLDebugTool.Scripts.Utilities;
using UnityEngine;

namespace MLDebugTool.Scripts
{
    public class TimeScaleUtility : Singleton<TimeScaleUtility>
    {
#if UNITY_EDITOR
        private const float TIMESCALE_UPPER_LIMIT = 100f;
#else
        private const float TIMESCALE_UPPER_LIMIT = float.PositiveInfinity;
#endif
        public static event Action<TimeScaleUtility> OnTimescaleChanged;
        
        [SerializeField]
        private float timeScaleStep = 1f;

        private float targetTimeScale = 1;
        public float TargetTimeScale
        {
            get => targetTimeScale;
            private set
            {
                if (targetTimeScale != value)
                {
                    targetTimeScale = value;
                    OnTimescaleChanged?.Invoke(this);
                }
            }
        }
        
        private float previousTimeScale;
        private float lerpDuration = 1;
        private float elapsedLerpTime;
        private float gameplayTimeScale;
        
        protected override void Awake()
        {
            base.Awake();
            Time.timeScale = 1;
            gameplayTimeScale = Time.timeScale;
        }

        private void OnDestroy()
        {
            ResetTimeScale();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                UpdatePreviousTimeScale();
                TargetTimeScale = Mathf.Min(TargetTimeScale + timeScaleStep, TIMESCALE_UPPER_LIMIT);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                UpdatePreviousTimeScale();
                TargetTimeScale = Mathf.Max(TargetTimeScale - timeScaleStep, 0f);
            }

            if (!Time.timeScale.Equals(TargetTimeScale))
            {
                float lerpProgress = Mathf.Clamp01(lerpDuration <= 0 ? 1 : elapsedLerpTime / lerpDuration);
                Time.timeScale = Mathf.Lerp(previousTimeScale, TargetTimeScale, lerpProgress);
                elapsedLerpTime += Time.unscaledDeltaTime;
                gameplayTimeScale = Time.timeScale;
            }
        }

        private void UpdatePreviousTimeScale()
        {
            previousTimeScale = Time.timeScale;
        }
        
        public void SetTimeScale(float timeScaleToSet)
        {
            TargetTimeScale = timeScaleToSet;
            Time.timeScale = timeScaleToSet;
        }

        /// <summary>
        /// Resets the time scale
        /// </summary>
        /// <param name="resetToLastGameplay">If true, resets the time scale to the last gameplay</param>
        public void ResetTimeScale(bool resetToLastGameplay = false)
        {
            Time.timeScale = resetToLastGameplay ? gameplayTimeScale : 1;

            if (!resetToLastGameplay)
            {
                TargetTimeScale = 1;
                gameplayTimeScale = Time.timeScale;
            }
        }
    }
}