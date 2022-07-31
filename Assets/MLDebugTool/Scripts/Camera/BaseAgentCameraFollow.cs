using MLDebugTool.Scripts.Agent;
using MLDebugTool.Scripts.Utilities;
using UnityEngine;

namespace MLDebugTool.Scripts.Camera
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

        private void HandleNewAgentSelected(AgentSelector agentSelector, DebuggableAgent debuggableAgent)
        {
            agentToFollow = debuggableAgent;
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