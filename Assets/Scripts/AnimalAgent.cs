using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net;
using UnityEngine.UIElements;
using System.Linq;

public class AnimalAgent : Agent
{
    [Tooltip("Start zero")] public int animalNumber;

    private Rigidbody rigid;

    private Vector3 startPosition;
    private Quaternion startRotation;
    public Transform[] startTransforms;
    private Vector3[] startPositions;
    private Quaternion[] startRotations;
    [Tooltip("Inference Only")] public bool useModel;

    private int moveZ = 0;
    private int turnX = 0;
    public float moveSpeed;
    public float turnSpeed;

    private List<GameObject> checkpoints;
    private int nextCheckpoint = 0;

    private Vector3 checkpointForward;
    private float directionDot;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

        startPosition = rigid.position;
        startRotation = rigid.rotation;
        startPositions = startTransforms.Select(t => t.position).ToArray();
        startRotations = startTransforms.Select(t => t.rotation).ToArray();

        checkpoints = GameManager.Instance.checkpoints.checkpoints;
    }

    public override void OnEpisodeBegin()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        if (useModel) {
            // Use a completed model
            rigid.position = startPosition;
            rigid.rotation = startRotation;
        }
        else {
            int randomPosition;
            if (startPositions.Length > 0) {
                // Real course
                randomPosition = UnityEngine.Random.Range(0, startPositions.Length);
                rigid.position = startPositions[randomPosition];
                rigid.rotation = startRotations[randomPosition];
            }
            else {
                // Left or Right course
                randomPosition = UnityEngine.Random.Range(0, 4);
                rigid.position = new Vector3(startPosition.x, startPosition.y, -4 + randomPosition * 2.3f);
                rigid.rotation = startRotation;
            }
        }
        
        nextCheckpoint = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(rigid.position);
        // sensor.AddObservation(rigid.rotation);

        // sensor.AddObservation(rigid.velocity);
        // sensor.AddObservation(rigid.angularVelocity);

        // üũ����Ʈ ���� ��ġ��
        checkpointForward = nextCheckpoint < checkpoints.Count ?
            checkpoints[nextCheckpoint].transform.forward : Vector3.zero;
        directionDot = Vector3.Dot(checkpointForward, transform.forward);
        sensor.AddObservation(checkpointForward != Vector3.zero ? directionDot : 1);

        // üũ����Ʈ���� �Ÿ�
        // float distanceToCheckpoint = Vector3.Distance(transform.position, GameManager.Instance.checkpoints.checkpoints[nextCheckpoint].transform.position);
        // sensor.AddObservation(distanceToCheckpoint);

        // üũ����Ʈ���� �¿� ����
        // Vector3 directionToCheckpoint = checkpoints[nextCheckpoint].transform.position - transform.position;
        // directionToCheckpoint.y = 0; // Y���� �������� ȸ���ϵ��� Y ��ǥ�� ����
        // Vector3 rotationAxis = Vector3.Cross(transform.forward, directionToCheckpoint.normalized);
        // sensor.AddObservation(rotationAxis);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        moveZ = actions.DiscreteActions[0]; // 0: ����, 1: ��, 2: ��
        turnX = actions.DiscreteActions[1]; // 0: ����, 1: ����, 2: ��


        // �ð��� ���Ƽ
        // AddReward(MaxStep != 0 ? -1f / MaxStep : 0);
        AddReward(-1f / 1000);

        // �ӵ� ��� ����
        // float speedReward = Mathf.Clamp(Mathf.Sqrt(rigid.velocity.magnitude) * 0.01f, 0, Mathf.Abs(stepPenalty / 2));
        // AddReward(speedReward);

        // ������ ���� ���� ��ġ�� ����
        if (directionDot < 0 || directionDot > 0.9f) {
            AddReward(directionDot * (moveZ == 1 ? 1 : 0) * 0.01f);
        }

        // üũ����Ʈ���� �Ÿ� ����
        // float distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[nextCheckpoint].transform.position);
        // AddReward(-distanceToCheckpoint * 0.0001f);

        // ���� ���� ���� ���
        // Debug.Log($"Current Reward: {GetCumulativeReward()} // {nextCheckpoint}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteSegment = actionsOut.DiscreteActions;

        float moveZ = Input.GetAxisRaw("Vertical");
        float turnX = Input.GetAxisRaw("Horizontal");
        moveZ = moveZ == -1 ? 2 : moveZ;
        turnX = turnX == -1 ? 2 : turnX;

        discreteSegment[0] = (int)moveZ;
        discreteSegment[1] = (int)turnX;
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = transform.forward * (moveZ == 2 ? -1 : moveZ) * moveSpeed;
        rigid.velocity = moveDirection;

        float turnDirection = (turnX == 2 ? -1 : turnX) * turnSpeed;
        rigid.angularVelocity = new Vector3(0f, turnDirection, 0f);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint")) {
            if (nextCheckpoint == checkpoints.IndexOf(other.gameObject)) {
                AddReward(2f); // üũ����Ʈ ����
                nextCheckpoint++;

                // Debug.Log($"Current Reward: {GetCumulativeReward()} // {nextCheckpoint}");

                if (nextCheckpoint >= checkpoints.Count) {
                    AddReward(20f); // ���� ����
                    if (GameManager.Instance.mainCamera.isTraining) {
                        EndEpisode();
                    }
                    else {
                        nextCheckpoint = 0;
                        SetReward(0);
                    }
                }
            }
            else {
                AddReward(-1f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-1f);
            // OnEpisodeBegin();
            // EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Animal")) {
            AddReward(-0.05f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.1f);

            if (GetCumulativeReward() < -20f) {
                AddReward(-10f);
                if (GameManager.Instance.mainCamera.isTraining) {
                    EndEpisode();
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(0.5f);
        }
    }
}
