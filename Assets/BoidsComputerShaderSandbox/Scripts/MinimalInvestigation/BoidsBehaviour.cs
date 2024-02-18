using UnityEngine;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public class BoidsBehaviour : MonoBehaviour
    {
        private BoidsCore _boidsCore;

        [SerializeField]
        private GameObject boidPrefab;

        private Transform[] _boidsTransforms;

        private void Start()
        {
            if (boidPrefab == null)
            {
                Debug.LogError("boid prefab is null");
                return;
            }

            _boidsCore = new BoidsCore(new BoidsOptions
            {
                Count = 50,
                InsightRange = 1f,
                BoundingSize = new Vector3(5f, 5f, 5f),
                MaxAcceleration = 0.2f,
                MaxVelocity = 0.5f
            });

            _boidsTransforms = new Transform[_boidsCore.Count];
            for (var i = 0; i < _boidsCore.Count; i++)
            {
                _boidsTransforms[i] = Instantiate(boidPrefab).transform;
            }
        }

        private void Update()
        {
            if (_boidsTransforms == null)
            {
                Debug.LogError("boids is null!");
                return;
            }

            _boidsCore.Update(Time.deltaTime);
            for (var i = 0; i < _boidsCore.Count; i++)
            {
                _boidsTransforms[i].transform.position = _boidsCore.Positions[i];
            }
        }
    }
}