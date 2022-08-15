using System;
using System.Collections.Generic;
using AgentDebugTool.Scripts.Agent;
using UnityEngine;

namespace AgentDebugTool.Scripts
{
    public class AgentSelector : MonoBehaviour
    {
        public static event Action<AgentSelector, DebugAgent> OnNewAgentSelected;
        
        private int currentIndex = -1;
        private readonly List<DebugAgent> activeAgents = new List<DebugAgent>();

        private void Awake()
        { 
            DebugAgent.OnAnyAgentEnabled += HandleAnyAgentEnabled;
            DebugAgent.OnAnyAgentDisabled += HandleAnyAgentDisabled;
            DebugAgent.OnAnyAgentDestroyed += HandleAnyAgentDestroyed;
        }

        private void OnDisable()
        {
            DebugAgent.OnAnyAgentEnabled -= HandleAnyAgentEnabled;
            DebugAgent.OnAnyAgentDisabled -= HandleAnyAgentDisabled;
            DebugAgent.OnAnyAgentDestroyed -= HandleAnyAgentDestroyed;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int newIndex = currentIndex - 1;
                SetNewIndex(newIndex < 0 ? activeAgents.Count - 1 : newIndex);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int newIndex = currentIndex + 1;
                SetNewIndex(newIndex >= activeAgents.Count ? 0 : newIndex);
            }
        }

        private void SetNewIndex(int index)
        {
            if (index >= activeAgents.Count || activeAgents.Count == 0 || index < 0)
            {
                return;
            }

            currentIndex = index;
            OnNewAgentSelected?.Invoke(this, activeAgents[currentIndex]);
        }

        #region Handlers

        private void HandleAnyAgentEnabled(DebugAgent debugAgent)
        {
            if (!activeAgents.Contains(debugAgent))
            {
                activeAgents.Add(debugAgent);
                if (currentIndex < 0)
                {
                    SetNewIndex(0);
                }
            }
        }

        private void HandleAnyAgentDestroyed(DebugAgent debugAgent)
        {
            if (activeAgents.Contains(debugAgent))
            {
                activeAgents.Remove(debugAgent);
            }

            int newIndex = activeAgents.Count - 1 > currentIndex ? currentIndex : currentIndex - 1;
            SetNewIndex(newIndex);
        }
        
        private void HandleAnyAgentDisabled(DebugAgent debugAgent)
        {
            if (activeAgents.Contains(debugAgent))
            {
                activeAgents.Remove(debugAgent);
            }
            
            int newIndex = activeAgents.Count - 1 > currentIndex ? currentIndex : currentIndex - 1;
            SetNewIndex(newIndex);
        }

        #endregion
    }
}