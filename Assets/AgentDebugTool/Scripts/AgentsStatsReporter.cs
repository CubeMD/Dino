using AgentDebugTool.Scripts.Agent;
using Unity.MLAgents;
using UnityEngine;

namespace AgentDebugTool.Scripts
{
    public class AgentsStatsReporter : MonoBehaviour
    {
        private float totalGameplaySeconds;
        private int totalDecisions;
        private int totalEpisodes;

        private void Awake()
        {
            DebugAgent.OnAnyAgentDecisionRequested += HandleAnyAgentDecisionRequested;
            DebugAgent.OnAnyAgentEpisodeBegin += HandleAnyAgentEpisodeBegin;
        }

        private void OnDestroy()
        {
            DebugAgent.OnAnyAgentDecisionRequested -= HandleAnyAgentDecisionRequested;
            DebugAgent.OnAnyAgentEpisodeBegin -= HandleAnyAgentEpisodeBegin;
        }

        private void HandleAnyAgentDecisionRequested(DebugAgent debugAgent, float time)
        {
            totalDecisions++;
            totalGameplaySeconds += time;
            Academy.Instance.StatsRecorder.Add("Totals/Decisions", totalDecisions, StatAggregationMethod.MostRecent);
            Academy.Instance.StatsRecorder.Add("Totals/GameplaySeconds", totalGameplaySeconds, StatAggregationMethod.MostRecent);
        }
        
        private void HandleAnyAgentEpisodeBegin(DebugAgent debugAgent)
        {
            totalEpisodes++;
            Academy.Instance.StatsRecorder.Add("Totals/Episodes", totalEpisodes, StatAggregationMethod.MostRecent);
        }
    }
}