using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools
{
    /// <summary>
    /// Duplicates the environment rightwards, taking into account bounds and safety distance.
    /// Duplicated environments can be placed on separate scenes with non interacting physics.
    /// NumEnvironments and MultiScene parameters are overriden with a hyperparameter provided in the python config file
    /// (through the academy environment parameters)
    /// </summary>
    public class EnvironmentDuplicator : MonoBehaviour
    {
        [SerializeField] private GameObject environment;
    
        [SerializeField] [Tooltip("Distance between bounds of spawned environments")]
        private float safetyDistance = 50;
    
        [SerializeField] [Tooltip("Overriden by python config")]
        private int defaultNumEnvironments = 10;
    
        [SerializeField] [Tooltip("Overriden by python config")]
        private bool defaultMultiScene;

        private bool multiScene;
        private readonly List<PhysicsScene> spawnedPhysicsScenes = new List<PhysicsScene>(); 
        
        private void Awake()
        {
            int numEnvironments = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("environments_per_unity_process", defaultNumEnvironments);
            int multiSceneValue = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("multi_scene", -1f);
            multiScene = multiSceneValue == -1 ? defaultMultiScene : multiSceneValue == 1;
        
            CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
            Bounds environmentBounds = CalculateEnvironmentBounds(environment);
        
            Vector3 nextEnvironmentOffset = Vector3.right * (environmentBounds.size.x + safetyDistance);
            Vector3 firstEnvironmentPosition = environment.transform.position;
        
            for (int i = 0; i < numEnvironments; i++)
            {
                Vector3 pos = firstEnvironmentPosition + nextEnvironmentOffset * i;
                GameObject env = Instantiate(environment, pos, environment.transform.rotation);

                if (multiScene)
                {
                    Scene scene = SceneManager.CreateScene($"SpawnedEnv-{i}", csp);
                    SceneManager.MoveGameObjectToScene(env.gameObject, scene);
                    spawnedPhysicsScenes.Add(scene.GetPhysicsScene());
                }
            }
        }

        private void FixedUpdate()
        {
            foreach (PhysicsScene spawnedPhysicsScene in spawnedPhysicsScenes)
            {
                spawnedPhysicsScene.Simulate(Time.fixedDeltaTime);
            }
        }
    
        private static Bounds CalculateEnvironmentBounds(GameObject go)
        {
            Bounds bounds = new Bounds();

            foreach (BoxCollider col in go.GetComponentsInChildren<BoxCollider>())
            {
                Bounds b = new Bounds
                {
                    center = col.transform.position,
                    size = new Vector3( // Is it correct?
                        col.size.x * col.transform.lossyScale.x,
                        col.size.y * col.transform.lossyScale.y,
                        col.size.z * col.transform.lossyScale.z)
                };
                bounds.Encapsulate(b);
            }

            return bounds;
        }
    }
}