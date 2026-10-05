using UnityEngine;

public class ResetLearningLab : MonoBehaviour
{
    [SerializeField] private Rigidbody cube;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private void Start()
    {
        startingPosition = cube.position;
        startingRotation = cube.rotation;
    }

    public void ResetCube()
    {
        cube.linearVelocity = Vector3.zero;
        cube.angularVelocity = Vector3.zero;

        cube.position = startingPosition;
        cube.rotation = startingRotation;
    }
}
