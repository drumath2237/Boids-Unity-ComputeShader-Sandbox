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
        private Vector3 boundingSize;

        [SerializeField]
        private float timeScale = 1f;

        [SerializeField]
        private float fleeThreshold = 1f;


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
                InsightRange = insightRange,
                BoundingSize = boundingSize,
                MaxAcceleration = maxAcceleration,
                MaxVelocity = maxVelocity,
                FleeThreshold = fleeThreshold
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

            _boidsCore.Update(Time.deltaTime * timeScale);
            for (var i = 0; i < _boidsCore.Count; i++)
            {
                _boidsTransforms[i].transform.position = _boidsCore.Positions[i];
                _boidsTransforms[i].transform.rotation = Quaternion.LookRotation(_boidsCore.Velocities[i]);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, boundingSize * 2);
        }
    }
}