using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace MLDebugTool.Scripts.Agent
{
    public abstract class DebuggableAgent : Unity.MLAgents.Agent
    {
        public static event Action<DebuggableAgent> OnAnyAgentEnabled;
        public static event Action<DebuggableAgent> OnAnyAgentDisabled;
        public static event Action<DebuggableAgent> OnAnyAgentDestroyed;
        public static event Action<DebuggableAgent> OnAnyAgentEpisodeBegin;
        public static event Action<DebuggableAgent, float> OnAnyAgentDecisionRequested;
        public event Action<DebuggableAgent, ActionBuffers> OnAgentDecisionRequested;
        public event Action<DebuggableAgent, Dictionary<string, string>> OnAgentObservationsCollected;
        
        protected readonly Dictionary<string, string> observationsDebugSet = new Dictionary<string, string>();
        private readonly Dictionary<string, float> episodeStats = new Dictionary<string, float>();

        protected float timeOfEpisodeStart;
        protected float timeOfLastDecision;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnAnyAgentEnabled?.Invoke(this);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            OnAnyAgentDisabled?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            OnAnyAgentDestroyed?.Invoke(this);
        }

        public override void OnEpisodeBegin()
        {
            SendEndEpisodeStatsAndClear();
            
            OnAnyAgentEpisodeBegin?.Invoke(this);

            timeOfEpisodeStart = timeOfLastDecision = Time.time;
        }

        private void SendEndEpisodeStatsAndClear()
        {
            foreach (string key in episodeStats.Keys)
            {
                Academy.Instance.StatsRecorder.Add(key, episodeStats[key], StatAggregationMethod.Histogram);
            }

            HashSet<string> keys = new HashSet<string>(episodeStats.Keys);
            foreach (string key in keys)
            {
                episodeStats[key] = 0;
            }
        }
        
        protected void RecordEpisodeStat(string statName, float value = 1f)
        {
            if (episodeStats.ContainsKey(statName))
            {
                episodeStats[statName] += value;
            }
            else
            {
                episodeStats.Add(statName, value);
            }
        }
        
        public override void OnActionReceived(ActionBuffers actions)
        {
            OnAnyAgentDecisionRequested?.Invoke(this, Time.time - timeOfLastDecision);
            OnAgentDecisionRequested?.Invoke(this, actions);
            
            RecordEpisodeStat("PerEpisode/Decisions");
            
            timeOfLastDecision = Time.time;
        }

        protected void BroadcastObservationsCollected()
        {
            OnAgentObservationsCollected?.Invoke(this, observationsDebugSet);
        }
    }
}