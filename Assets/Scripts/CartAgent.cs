using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CartAgent : Agent
{
    private void Start()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        // base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // base.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // base.OnActionReceived(actions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // base.Heuristic(actionsOut);
    }

    private void FixedUpdate()
    {
        
    }

    // Animal == Obstacle
}
