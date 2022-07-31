using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MLDebugTool.Scripts.Trackers
{
    /// <summary>
    /// Base updatable tracker class
    /// Utilizes the update coroutine in order to update the tracker in intervals
    /// </summary>
    public abstract class BaseTracker
    {
        protected const char NEW_LINE = '\n';
        protected const string COLOR_TEXT_START = "<color=#{0}>";
        protected const string COLOR_TEXT_END = "</color>";
        
        protected virtual float UpdateInterval => 0.1f;

        protected readonly MlDebugTool mlDebugTool;
        private readonly Text counterText;
        
        protected StringBuilder text;
        protected string colorCached;
        private bool isInitialized;
        private bool isActivated;
        protected bool dirty;
        private Coroutine updateCoroutine;
        private WaitForSeconds cachedWaitForSeconds;
        
        /// <summary>
        /// Sets the reference to ml debug tool and subscribes to its events
        /// </summary>
        /// <param name="reference">The ML debug tool</param>
        /// <param name="counterText">The counter text filed to display the data</param>
        protected BaseTracker(MlDebugTool reference, Text counterText)
        {
            mlDebugTool = reference;
            this.counterText = counterText;
        }

        #region Activation
        
        /// <summary>
        /// Simulates the OnEnable method
        /// Called on FPSCounter enabled
        /// </summary>
        public void Activate()
        {
            // Creates a new string builder or resets the existing one
            text = new StringBuilder(500);

            // Caches the normal color
            if (colorCached == null)
            {
                CacheCurrentColor();
            }

            if (!isActivated)
            {
                // Invokes the activation actions
                // Must be implemented in the inherited class
                ActivationActions();
            }

            // Invokes the initialization actions
            // If wasn't initialized before 
            // Must be implemented in the inherited class
            if (!isInitialized)
            {
                InitActions();
            }
            
            // Updates and displays the value without force 
            UpdateValueAndDisplay(false);
            
            isActivated = true;
        }

        /// <summary>
        /// Method for overriden logic on counter activation
        /// </summary>
        protected virtual void ActivationActions()
        {
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
        
        /// <summary>
        /// Method for overriden logic on counter initialization.
        /// Should call InitActions.base()
        /// </summary>
        protected virtual void InitActions()
        {
            isInitialized = true;
            
            // Starts the update coroutine
            StartUpdateCoroutine();
        }
        
        /// <summary>
        /// Starts the update coroutine and saves it to updateCoroutine
        /// </summary>
        private void StartUpdateCoroutine()
        {
            updateCoroutine = mlDebugTool.StartCoroutine(UpdateCounter());
        }

        #endregion

        #region Update

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
        
        /// <summary>
        /// Invokes the update value method and displays the result text to the field
        /// </summary>
        /// <param name="force">If true will be forced to update the counter parameters</param>
        protected void UpdateValueAndDisplay(bool force)
        {
            // Updates the value with given force parameter
            UpdateValue(force);

            // Displays the counter text if the current operation mode is Normal and text field isn't null
            if (counterText != null)
            {
                DisplayText();
            }
        }

        /// <summary>
        /// Abstract update value method. All counter logic should be placed here
        /// </summary>
        /// <param name="force">If true will be forced to update the counter parameters</param>
        protected abstract void UpdateValue(bool force);
        
        /// <summary>
        /// Displays the current text value to the text field
        /// If the text field is null or current operation mode isn't normal will not proceed
        /// </summary>
        private void DisplayText()
        {
            counterText.text = text.ToString();
            dirty = false;
        }
        
        #endregion

        #region Deactivation

        /// <summary>
        /// Simulates OnDisable
        /// Called when MLDebugTool is disabled
        /// Should call InitActions.base()
        /// </summary>
        public void Deactivate()
        {
            // Return if wasn't initialized
            if (!isInitialized || !isActivated) 
                return;

            // Removes the text from the text field if exists
            text?.Remove(0, text.Length);

            // Clears the canvas texts
            ClearCanvasText();

            // Invokes the deactivation actions
            // Must be implemented in the inherited class
            DeactivationActions();

            // Sets as not initialized and not activated
            isInitialized = false;
            isActivated = false;
        }
        
        /// <summary>
        /// Clears the canvas text to empty string
        /// Should be overridden with the base reference
        /// </summary>
        public virtual void ClearCanvasText()
        {
            if(counterText != null)
            {
                counterText.text = "";
            }
        }

        /// <summary>
        /// Method for overriden logic on counter deactivation
        /// </summary>
        protected virtual void DeactivationActions()
        {
            // Stops the update coroutine
            StopUpdateCoroutine();
        }
        
        /// <summary>
        /// If updateCoroutine is exist, stops the update coroutine
        /// </summary>
        private void StopUpdateCoroutine()
        {
            if(updateCoroutine != null && mlDebugTool != null)
            {
                mlDebugTool.StopCoroutine(updateCoroutine);
            }
        }
        
        /// <summary>
        /// Simulates the OnDestroy method
        /// Called when MLDebugTool is destroyed
        /// Should call InitActions.base()
        /// </summary>
        public void Terminate()
        {
            if (text != null)
            {
                text.Remove(0, text.Length);
                text = null;
            }
        }
        
        #endregion
        
        #region Cache Color
        
        /// <summary>
        /// Caches the color HTML tag to avoid extra allocations
        /// </summary>
        protected abstract void CacheCurrentColor();
        
        /// <summary>
        /// Converts the RGBA color 32 bit representation to hexadecimal color
        /// </summary>
        /// <param name="color">Color32 to covert</param>
        /// <returns>HEX color</returns>
        internal string Color32ToHex(Color32 color)
        {
            return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
        }
        
        #endregion
    }
}