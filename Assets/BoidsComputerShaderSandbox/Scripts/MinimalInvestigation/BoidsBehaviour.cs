using UnityEngine;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public class BoidsBehaviour : MonoBehaviour
    {
        private BoidsCore _boidsCore;

        [SerializeField]
        private GameObject boidPrefab;

        private Transform[] _boidsTransforms;

        [Space, Header("Boids Info")]
        [SerializeField]
        private int boidsCount = 50;

        [SerializeField]
        private float insightRange = 3f;

        [SerializeField]
        private float maxVelocity = 0.1f;

        [SerializeField]
        private float maxAcceleration = 0.1f;

        [SerializeField]
        private Vector3 boundarySize;

        [SerializeField]
        private float timeScale = 1f;

        [SerializeField]
        private float fleeThreshold = 1f;

        [Space, Header("Force Weights")]
        [SerializeField]
        private float alignWeight = 1f;

        [SerializeField]
        private float separationWeight = 1f;

        [SerializeField]
        private float cohesionWeight = 1f;

        private void Start()
        {
            if (boidPrefab == null)
            {
                Debug.LogError("boid prefab is null");
                return;
            }

            _boidsCore = new BoidsCore(new BoidsOptions
            {
                Count = boidsCount,
                InitPositionRange = boundarySize,
                InitMaxAcceleration = maxAcceleration,
                InitMaxVelocity = maxVelocity,
            });

            _boidsTransforms = new Transform[_boidsCore.Count];
            for (var i = 0; i < _boidsCore.Count; i++)
            {
                _boidsTransforms[i] = Instantiate(boidPrefab, gameObject.transform).transform;
            }
        }

        private void Update()
        {
            if (_boidsTransforms == null)
            {
                Debug.LogError("boids is null!");
                return;
            }

            var updateParams = new UpdateParams
            {
                AlignWeight = alignWeight,
                SeparationWeight = separationWeight,
                CohesionWeight = cohesionWeight,
                FleeThreshold = fleeThreshold,
                InsightRange = insightRange,
                MaxAcceleration = maxAcceleration,
                MaxVelocity = maxVelocity,
                BoundarySize = boundarySize
            };

            _boidsCore.Update(Time.deltaTime * timeScale, updateParams);
            for (var i = 0; i < _boidsCore.Count; i++)
            {
                _boidsTransforms[i].transform.position = _boidsCore.Boids[i].Position;
                _boidsTransforms[i].transform.rotation = Quaternion.LookRotation(_boidsCore.Boids[i].Velocity);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, boundarySize * 2);
        }
    }
}