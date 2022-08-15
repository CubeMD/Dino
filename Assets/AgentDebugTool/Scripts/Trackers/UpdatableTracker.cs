using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AgentDebugTool.Scripts.Trackers
{
    public abstract class UpdatableTracker : BaseTracker
    {
        protected virtual float UpdateInterval => 0.1f;
        
        private Coroutine updateCoroutine;
        private WaitForSeconds cachedWaitForSeconds;
        
        protected UpdatableTracker(AgentDebugTool reference, Text counterText, Color defaultColor) : base(reference, counterText, defaultColor) { }

        protected override void ActivationActions()
        {
            base.ActivationActions();
            // Caches wait for second update interval
            CacheWaitForSeconds();
        }

        /// <summary>
        /// Caches the wait for seconds update interval
        /// </summary>
        private void CacheWaitForSeconds()
        {
            cachedWaitForSeconds = new WaitForSeconds(UpdateInterval);
        }

        protected override void InitActions()
        {
            // Starts the update coroutine
            StartUpdateCoroutine();
            base.InitActions();
        }

        /// <summary>
        /// Starts the update coroutine and saves it to updateCoroutine
        /// </summary>
        private void StartUpdateCoroutine()
        {
            updateCoroutine = agentDebugTool.StartCoroutine(UpdateCounter());
        }
        
        /// <summary>
        /// Update counter routine that waits for cached update interval and invokes the update and display method
        /// Must be overriden for more complex behaviour
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator UpdateCounter()
        {
            while (true)
            {
                yield return cachedWaitForSeconds;

                // Updates and displays the zone
                UpdateValueAndDisplay(false);
            }
        }

        protected override void DeactivationActions()
        {
            base.DeactivationActions();
            // Stops the update coroutine
            StopUpdateCoroutine();
        }

        /// <summary>
        /// If updateCoroutine is exist, stops the update coroutine
        /// </summary>
        private void StopUpdateCoroutine()
        {
            if(updateCoroutine != null && agentDebugTool != null)
            {
                agentDebugTool.StopCoroutine(updateCoroutine);
            }
        }
    }
}