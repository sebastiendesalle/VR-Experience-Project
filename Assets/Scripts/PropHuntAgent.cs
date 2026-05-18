using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveCubeAgent : Agent
{
    public override void OnEpisodeBegin()
    {

    }
    public override void CollectObservations(VectorSensor sensor)
    {

    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    { 

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
