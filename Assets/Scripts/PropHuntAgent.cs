using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class PropHuntAgent : Agent
{
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    public float attackRange = 2f;

    private StartEpisode _envManager;
    private HashSet<int> _visitedSections = new HashSet<int>();

    public override void Initialize()
    {
        _envManager = GetComponentInParent<StartEpisode>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_visitedSections.Count);
    }

    public override void OnEpisodeBegin()
    {
        _visitedSections.Clear();
        _envManager.Begin();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //tijdstraf
        AddReward(-0.0001f);

        // beweging
        float turnInput = actionBuffers.ContinuousActions[0];
        float moveInput = actionBuffers.ContinuousActions[1];

        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        CheckIfFallenOver();

        CheckForNewSection();

        if (actionBuffers.DiscreteActions[0] == 1)
        {
            PerformAttack();
        }
    }

    private void CheckIfFallenOver()
    {
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f)
        {
            Debug.Log("AI is omgevallen!");
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private void PerformAttack()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Gevonden");
                SetReward(1.0f);
                EndEpisode();
            }
            else if (hit.collider.CompareTag("Prop"))
            {
                Debug.Log("fake prop");
                AddReward(-0.01f);
            }
        }
    }

    private void CheckForNewSection()
    {
        for (int i = 0; i < _envManager.SectionMiddleCoordinates.Length; i++)
        {
            float dist = Vector3.Distance(transform.localPosition, _envManager.SectionMiddleCoordinates[i]);

            if (dist < _envManager.SectionOffset - 10 && !_visitedSections.Contains(i))
            {
                _visitedSections.Add(i);
                Debug.Log($"Nieuwe sectie ontdekt: {i}");
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.E) ? 1 : 0;
    }
}