using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Net;

public class AnimalAgent : Agent
{
    private Rigidbody rigid;

    public float moveSpeed;
    public float turnSpeed;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rigid.position = Vector3.zero;
        rigid.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rigid.velocity);
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveZ = actions.DiscreteActions[0]; // 0: 정지, 1: 앞, 2: 뒤
        float moveX = actions.DiscreteActions[1]; // 0: 정지, 1: 오른, 2: 왼

        Vector3 newPosition = Vector3.MoveTowards(rigid.position, transform.forward * (moveZ == 2 ? -1 : moveZ),
            moveSpeed * Time.deltaTime);
        rigid.MovePosition(newPosition);

        float turnY = (moveX == 2 ? -1 : moveX) * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnY, 0f);
        rigid.MoveRotation(rigid.rotation * turnRotation);

        AddReward(-1f / MaxStep); // 빠르게 목표에 다다를 수 있도록 부정적인 보상을 지속적으로
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
        if (collision.gameObject.CompareTag("Checkpoint")) {
            AddReward(1f);
        }
        else if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) {
            AddReward(-0.1f);
        }
    }
}
