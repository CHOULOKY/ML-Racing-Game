using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net;
using UnityEngine.UIElements;

public class AnimalAgent : Agent
{
    private Rigidbody rigid;

    private Vector3 startPosition;
    private Quaternion startRotation;

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

        checkpoints = GameManager.Instance.checkpoints.checkpoints;
    }

    public override void OnEpisodeBegin()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        int randomPosition = UnityEngine.Random.Range(0, 4);
        rigid.position = new Vector3(startPosition.x, startPosition.y, -4 + randomPosition * 2.3f);
        // rigid.position = startPosition;
        rigid.rotation = startRotation;
        
        nextCheckpoint = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(rigid.position);
        // sensor.AddObservation(rigid.rotation);

        // sensor.AddObservation(rigid.velocity);
        // sensor.AddObservation(rigid.angularVelocity);

        // 체크포인트 방향 일치도
        checkpointForward = nextCheckpoint < checkpoints.Count ?
            checkpoints[nextCheckpoint].transform.forward : Vector3.zero;
        directionDot = Vector3.Dot(checkpointForward, transform.forward);
        sensor.AddObservation(checkpointForward != Vector3.zero ? directionDot : 1);

        // 체크포인트와의 거리
        // float distanceToCheckpoint = Vector3.Distance(transform.position, GameManager.Instance.checkpoints.checkpoints[nextCheckpoint].transform.position);
        // sensor.AddObservation(distanceToCheckpoint);

        // 체크포인트로의 좌우 방향
        // Vector3 directionToCheckpoint = checkpoints[nextCheckpoint].transform.position - transform.position;
        // directionToCheckpoint.y = 0; // Y축을 기준으로 회전하도록 Y 좌표는 무시
        // Vector3 rotationAxis = Vector3.Cross(transform.forward, directionToCheckpoint.normalized);
        // sensor.AddObservation(rotationAxis);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveZ = actions.DiscreteActions[0]; // 0: 정지, 1: 앞, 2: 뒤
        float turnX = actions.DiscreteActions[1]; // 0: 정지, 1: 오른, 2: 왼

        Vector3 moveDirection = transform.forward * (moveZ == 2 ? -1 : moveZ) * moveSpeed;
        rigid.velocity = moveDirection;

        float turnDirection = (turnX == 2 ? -1 : turnX) * turnSpeed;
        rigid.angularVelocity = new Vector3(0f, turnDirection, 0f);


        // 시간당 페널티
        AddReward(MaxStep != 0 ? -1f / MaxStep : 0);
        
        // 속도 비례 보상
        // float speedReward = Mathf.Clamp(Mathf.Sqrt(rigid.velocity.magnitude) * 0.01f, 0, Mathf.Abs(stepPenalty / 2));
        // AddReward(speedReward);

        // 움직일 때만 방향 일치도 보상
        if (directionDot < 0 || directionDot > 0.9f) {
            AddReward(directionDot * (moveZ == 1 ? 1 : 0) * 0.01f);
        }

        // 체크포인트와의 거리 보상
        // float distanceToCheckpoint = Vector3.Distance(transform.position, checkpoints[nextCheckpoint].transform.position);
        // AddReward(-distanceToCheckpoint * 0.0001f);

        // 현재 누적 보상 출력
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint")) {
            if (nextCheckpoint == checkpoints.IndexOf(other.gameObject)) {
                AddReward(2f); // 체크포인트 보상
                nextCheckpoint++;

                // Debug.Log($"Current Reward: {GetCumulativeReward()} // {nextCheckpoint}");

                if (nextCheckpoint >= checkpoints.Count) {
                    // nextCheckpoint = -1;
                    AddReward(20f); // 완주 보상
                    EndEpisode();
                }
            }
            else {
                // AddReward(-1f);
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
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.1f);

            if (GetCumulativeReward() < -20f) {
                AddReward(-10f);
                EndEpisode();
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
