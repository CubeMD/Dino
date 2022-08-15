using System;
using System.Collections;
using UnityEngine;

namespace Dino
{
    [Serializable]
    public class DelayedJump
    {
        public static event Action<DelayedJump> OnJumpDelayFinished;
          
        private DelayedDinoAgent agent;
        private Coroutine waitJumpRoutine;
        public float timeOfExecution;
        
        public DelayedJump(DelayedDinoAgent delayedDinoAgent, float delay)
        {
            agent = delayedDinoAgent;
            timeOfExecution = Time.time + delay;
            waitJumpRoutine = agent.StartCoroutine(WaitJumpRoutine(delay));
            Debug.Log($"Scheduled at {Time.time}, delay {delay}");
           // Debug.Log($"The jump has been scheduled at {Time.time} with delay of {delay}, should be executed at {timeOfExecution}");
        }
            
        private IEnumerator WaitJumpRoutine(float delay)
        {
            while (Time.time < timeOfExecution)
            {
                yield return null;
            }
            //yield return new WaitForSeconds(delay);
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
               //Debug.Log("Jump terminated");
                agent.StopCoroutine(waitJumpRoutine);
            }
        }
    }
}