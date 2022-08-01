using System;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

namespace MLDebugTool.Scripts.Agent
{
    public abstract class DebuggableAgent : Unity.MLAgents.Agent
    {
        public static event Action<DebuggableAgent> OnAnyAgentEnabled;
        public static event Action<DebuggableAgent> OnAnyAgentDisabled;
        public static event Action<DebuggableAgent> OnAnyAgentDestroyed;

        public event Action<DebuggableAgent, ActionBuffers> OnAgentDecisionRequested;
        public event Action<DebuggableAgent, Dictionary<string, string>> OnAgentObservationsCollected;

        protected readonly Dictionary<string, string> observationsDebugSet = new Dictionary<string, string>();

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