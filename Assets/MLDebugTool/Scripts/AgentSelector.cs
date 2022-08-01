﻿using System;
using System.Collections.Generic;
using MLDebugTool.Scripts.Agent;
using UnityEngine;

namespace MLDebugTool.Scripts
{
    public class AgentSelector : MonoBehaviour
    {
        public static event Action<AgentSelector, DebuggableAgent> OnNewAgentSelected;
        
        private int currentIndex = -1;
        private readonly List<DebuggableAgent> activeAgents = new List<DebuggableAgent>();

        private void Awake()
        { 
            DebuggableAgent.OnAnyAgentEnabled += HandleAnyAgentEnabled;
            DebuggableAgent.OnAnyAgentDisabled += HandleAnyAgentDisabled;
            DebuggableAgent.OnAnyAgentDestroyed += HandleAnyAgentDestroyed;
        }

        private void OnDisable()
        {
            DebuggableAgent.OnAnyAgentEnabled -= HandleAnyAgentEnabled;
            DebuggableAgent.OnAnyAgentDisabled -= HandleAnyAgentDisabled;
            DebuggableAgent.OnAnyAgentDestroyed -= HandleAnyAgentDestroyed;
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

        private void HandleAnyAgentEnabled(DebuggableAgent debuggableAgent)
        {
            if (!activeAgents.Contains(debuggableAgent))
            {
                activeAgents.Add(debuggableAgent);
                if (currentIndex < 0)
                {
                    SetNewIndex(0);
                }
            }
        }

        private void HandleAnyAgentDestroyed(DebuggableAgent debuggableAgent)
        {
            if (activeAgents.Contains(debuggableAgent))
            {
                activeAgents.Remove(debuggableAgent);
            }

            int newIndex = activeAgents.Count - 1 > currentIndex ? currentIndex : currentIndex - 1;
            SetNewIndex(newIndex);
        }
        
        private void HandleAnyAgentDisabled(DebuggableAgent debuggableAgent)
        {
            if (activeAgents.Contains(debuggableAgent))
            {
                activeAgents.Remove(debuggableAgent);
            }
            
            int newIndex = activeAgents.Count - 1 > currentIndex ? currentIndex : currentIndex - 1;
            SetNewIndex(newIndex);
        }

        #endregion
    }
}