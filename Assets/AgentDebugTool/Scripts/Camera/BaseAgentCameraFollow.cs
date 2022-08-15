using AgentDebugTool.Scripts.Agent;
using AgentDebugTool.Scripts.Utilities;
using UnityEngine;

namespace AgentDebugTool.Scripts.Camera
{
    public abstract class BaseAgentCameraFollow : MonoBehaviour
    {
        private Unity.MLAgents.Agent agentToFollow;

        protected Vector3 AgentPosition => agentToFollow != null ? agentToFollow.transform.position : Vector3.zero;
        protected Quaternion AgentRotation => agentToFollow != null ? agentToFollow.transform.rotation : Quaternion.identity;
        
        private void Awake()
        {
            AgentSelector.OnNewAgentSelected += HandleNewAgentSelected;
        }

        private void OnDestroy()
        {
            AgentSelector.OnNewAgentSelected -= HandleNewAgentSelected;
        }

        private void HandleNewAgentSelected(AgentSelector agentSelector, DebugAgent debugAgent)
        {
            agentToFollow = debugAgent;
            UpdatePosition();
        }

        protected abstract void UpdatePosition();
        
        protected Quaternion GetReferenceOrientation(Vector3 worldUp)
        {
            if (agentToFollow == null)
            {
                return Quaternion.LookRotation(Vector3.forward, worldUp);
            }
         
            Vector3 forward = (AgentRotation * Vector3.forward).ProjectOntoPlane(worldUp);
            return Quaternion.LookRotation(forward, worldUp);
        }
    }
}