using System;
using System.Collections;
using UnityEngine;

namespace Agent
{
    [Serializable]
    public class DelayedJump
    {
        public static event Action<DelayedJump> OnJumpDelayFinished;
          
        private DinoAgentContinuous agent;
        private Coroutine waitJumpRoutine;
        private float timeOfExecution;
        
        public DelayedJump(DinoAgentContinuous dinoAgentContinuous, float delay)
        {
            agent = dinoAgentContinuous;
            waitJumpRoutine = agent.StartCoroutine(WaitJumpRoutine(delay));
            timeOfExecution = Time.time + delay;
        }
            
        private IEnumerator WaitJumpRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnJumpDelayFinished?.Invoke(this);
            waitJumpRoutine = null;
        }
        
        public float GetRemainingTime()
        {
            return timeOfExecution - Time.time;
        }
        
        public void Terminate()
        {
            if(waitJumpRoutine != null)
            {
                agent.StopCoroutine(waitJumpRoutine);
            }
        }
    }
}