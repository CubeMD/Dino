using System;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

namespace MLDebugTool.Scripts.Agent
{
    public abstract class DebuggableAgent : Unity.MLAgents.Agent
    {
        public static event Action<DebuggableAgent> OnAgentEnabled;
        public static event Action<DebuggableAgent> OnAgentDisabled;
        public static event Action<DebuggableAgent> OnAgentDestroyed;

        public event Action<DebuggableAgent, ActionBuffers> OnAgentDecisionRequested;
        public event Action<DebuggableAgent, Dictionary<string, string>> OnAgentObservationsCollected;

        protected readonly Dictionary<string, string> observationsDebugSet = new Dictionary<string, string>();

        protected override void OnEnable()
        {
            base.OnEnable();
            OnAgentEnabled?.Invoke(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnAgentDisabled?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            OnAgentDestroyed?.Invoke(this);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            OnAgentDecisionRequested?.Invoke(this, actions);
        }

        protected void BroadcastObservationsCollected()
        {
            OnAgentObservationsCollected?.Invoke(this, observationsDebugSet);
        }
    }
}