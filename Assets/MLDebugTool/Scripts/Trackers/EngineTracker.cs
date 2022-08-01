using UnityEngine;
using UnityEngine.UI;

namespace MLDebugTool.Scripts.Trackers
{
    /// <summary>
    /// Engine information tracker
    /// Any statistic related to engine or game play can be updated here
    /// </summary>
    public class EngineTracker : UpdatableTracker
    {
        private const string LINE_START_TIMESCALE = "Timescale: x{0}";

        private float lastTimeScaleValue = 1;
        protected override float UpdateInterval => 1f;

        public EngineTracker(MlDebugTool reference, Text counterText) : base(reference, counterText) { }

        protected override void ActivationActions()
        {
            base.ActivationActions();
            
            // Unsubscribing to prevent double subscribing
            TimeScaleUtility.OnTimescaleChanged -= HandleTimeScaleChanged;
            TimeScaleUtility.OnTimescaleChanged += HandleTimeScaleChanged;

            // Initial display of timescale value
            if (TimeScaleUtility.Instance != null)
            {
                HandleTimeScaleChanged(TimeScaleUtility.Instance, Time.timeScale);
            }
        }

        protected override void DeactivationActions()
        {
            base.DeactivationActions();
            TimeScaleUtility.OnTimescaleChanged -= HandleTimeScaleChanged;
        }

        private void HandleTimeScaleChanged(TimeScaleUtility timeScale, float scale)
        {
            UpdateValueAndDisplay(true);
        }

        protected override void UpdateValue(bool force)
        {
            if (lastTimeScaleValue != Time.timeScale || force)
            {
                lastTimeScaleValue = Time.timeScale;
                dirty = true;
            }
            
            if (dirty || force)
            {
                //Resets the text builder length
                text.Length = 0;
                
                // Adds timescale information
                text.AppendFormat(LINE_START_TIMESCALE, lastTimeScaleValue);
                
                // Adds the color prefix
                // If any values were added to the text builder
                if (text.Length > 0)
                {
                    text.Insert(0, colorCached);
                    text.Append(COLOR_TEXT_END);
                }
            }
        }

        protected override void CacheCurrentColor()
        {
            colorCached = string.Format(COLOR_TEXT_START, Color32ToHex(Color.white));
        }
    }
}