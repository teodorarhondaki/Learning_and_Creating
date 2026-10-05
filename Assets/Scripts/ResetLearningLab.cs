using UnityEngine;

public class ResetLearningLab : MonoBehaviour
{
    [SerializeField] private Rigidbody cube;
    [SerializeField] private float startingHeight = 4f;
    [SerializeField] private float currentSpeed;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private void Start()
    {
        startingPosition = cube.position;
        startingRotation = cube.rotation;
    }

    private void FixedUpdate()
    {
        currentSpeed = cube.linearVelocity.magnitude;
    }

    public void ResetCube()
    {
        cube.linearVelocity = Vector3.zero;
        cube.angularVelocity = Vector3.zero;

        Vector3 resetPosition = startingPosition;
        resetPosition.y = startingHeight;
        cube.position = resetPosition;
        //cube.position = startingPosition;
        cube.rotation = startingRotation;
        ResetCube();
    }
}
