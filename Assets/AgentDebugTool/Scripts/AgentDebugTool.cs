using System.Collections.Generic;
using AgentDebugTool.Scripts.Trackers;
using UnityEngine;
using UnityEngine.UI;

namespace AgentDebugTool.Scripts
{
    public class AgentDebugTool : MonoBehaviour
    {
        [SerializeField]
        private Color defaultColor = Color.black;
        [SerializeField]
        private Text agentDebug;
        [SerializeField]
        private CanvasGroup agentDecisionText;
        [SerializeField]
        private Toggle pauseOnDecisionToggle;
        [SerializeField]
        private Text gameplayTextField;

        private readonly List<BaseTracker> trackers = new List<BaseTracker>();
        
        private void Awake()
        {
            // Initializes all counter types
            InitializeTrackers();
            ActivateTrackers();
        }

        private void InitializeTrackers()
        {
            // Initialize counters
            EngineTracker engineTracker = new EngineTracker(this, gameplayTextField, defaultColor);
            trackers.Add(engineTracker);
            
            AgentTracker agentTracker = new AgentTracker(this, agentDebug, defaultColor, agentDecisionText, pauseOnDecisionToggle);
            trackers.Add(agentTracker);
        }

        private void ActivateTrackers()
        {
            foreach (BaseTracker tracker in trackers)
            {
                tracker.Activate();
            }
        }

        private void OnDisable()
        {
            foreach (BaseTracker tracker in trackers)
            {
                tracker.Deactivate();
            }
        }

        private void OnDestroy()
        {
            foreach (BaseTracker tracker in trackers)
            {
                tracker.Terminate();
            }
            
            trackers.Clear();
        }
    }
}