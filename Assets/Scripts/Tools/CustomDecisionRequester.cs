using Unity.MLAgents;

namespace Tools
{
    /// <summary>
    /// Custom decision requester that overrides decision period with a hyperparameter provided in the python config file
    /// (through the academy environment parameters)
    /// </summary>
    public class CustomDecisionRequester : DecisionRequester
    {
        private void Start()
        {
            DecisionPeriod = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("decision_frequency", DecisionPeriod);
        }
    }
}

