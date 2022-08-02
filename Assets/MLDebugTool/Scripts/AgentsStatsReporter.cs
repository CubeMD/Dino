using MLDebugTool.Scripts.Agent;
using Unity.MLAgents;
using UnityEngine;

namespace MLDebugTool.Scripts
{
    public class AgentsStatsReporter : MonoBehaviour
    {
        private float totalGameplaySeconds;
        private int totalDecisions;
        private int totalEpisodes;

        private void Awake()
        {
            DebuggableAgent.OnAnyAgentDecisionRequested += HandleAnyAgentDecisionRequested;
            DebuggableAgent.OnAnyAgentEpisodeBegin += HandleAnyAgentEpisodeBegin;
        }

        private void OnDestroy()
        {
            DebuggableAgent.OnAnyAgentDecisionRequested -= HandleAnyAgentDecisionRequested;
            DebuggableAgent.OnAnyAgentEpisodeBegin -= HandleAnyAgentEpisodeBegin;
        }

        private void HandleAnyAgentDecisionRequested(DebuggableAgent debuggableAgent, float time)
        {
            totalDecisions++;
            totalGameplaySeconds += time;
            Academy.Instance.StatsRecorder.Add("Totals/Decisions", totalDecisions, StatAggregationMethod.MostRecent);
            Academy.Instance.StatsRecorder.Add("Totals/GameplaySeconds", totalGameplaySeconds, StatAggregationMethod.MostRecent);
        }
        
        private void HandleAnyAgentEpisodeBegin(DebuggableAgent debuggableAgent)
        {
            totalEpisodes++;
            Academy.Instance.StatsRecorder.Add("Totals/Episodes", totalEpisodes, StatAggregationMethod.MostRecent);
        }
    }
}