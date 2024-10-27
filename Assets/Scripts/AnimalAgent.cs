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

    private int nextCheckpoint = 0;
    private float directionDot;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

        startPosition = transform.position;
        startRotation = transform.rotation;
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
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);

        Vector3 checkpointForward = GameManager.Instance.checkpoints.checkpoints[nextCheckpoint].transform.forward;
        directionDot = Vector3.Dot(checkpointForward, transform.forward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveZ = actions.DiscreteActions[0]; // 0: 정지, 1: 앞, 2: 뒤
        float moveX = actions.DiscreteActions[1]; // 0: 정지, 1: 오른, 2: 왼

        Vector3 newPosition = Vector3.MoveTowards(rigid.position,
            rigid.position + transform.forward * (moveZ == 2 ? -1 : moveZ), moveSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(newPosition);

        float turnY = (moveX == 2 ? -1 : moveX) * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnY, 0f);
        rigid.MoveRotation(rigid.rotation * turnRotation);

        if (MaxStep != 0) AddReward(-1f / MaxStep); // 빠르게 목표에 다다를 수 있도록 부정적인 보상을 지속적으로
        AddReward(directionDot * 0.1f); // 다음 체크포인트의 방향과 일치할수록 보상 증가

        // 현재 누적 보상 출력
        // Debug.Log($"Current Reward: {GetCumulativeReward()}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteSegment = actionsOut.DiscreteActions;

        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");
        moveZ = moveZ == -1 ? 2 : moveZ;
        moveX = moveX == -1 ? 2 : moveX;

        discreteSegment[0] = (int)moveZ;
        discreteSegment[1] = (int)moveX;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Checkpoint") &&
            nextCheckpoint == GameManager.Instance.checkpoints.checkpoints.IndexOf(collision.gameObject)) {
            AddReward(1f);
            if (nextCheckpoint >= GameManager.Instance.checkpoints.checkpoints.IndexOf(collision.gameObject)) {
                // nextCheckpoint = -1;
                EndEpisode();
            }
            nextCheckpoint++;
        }
        else if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.1f);
        }
    }

    /*
    private void FixedUpdate()
    {
        // test code
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        Vector3 newPosition = Vector3.MoveTowards(rigid.position,
            rigid.position + transform.forward * (moveZ == 2 ? -1 : moveZ), moveSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(newPosition);

        float turnY = moveX * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnY, 0f);
        rigid.MoveRotation(rigid.rotation * turnRotation);
    }
    */
}
