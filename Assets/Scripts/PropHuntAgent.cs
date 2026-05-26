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
    private Vector3 _lastPosition;
    private int _stuckCounter = 0;

    public StartEpisode _envManager;
    private HashSet<int> _visitedSections = new HashSet<int>();

    // NEW: Variable to track distance for the Hot and Cold game
    private float _previousDistance;

    public override void Initialize()
    {
        // _envManager is assigned in the Inspector
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_visitedSections.Count);

        for (int i = 0; i < _envManager.SectionMiddleCoordinates.Length; i++)
        {
            sensor.AddObservation(_visitedSections.Contains(i) ? 1.0f : 0.0f);
            Vector3 relativePosition = transform.InverseTransformPoint(_envManager.SectionMiddleCoordinates[i]);
            sensor.AddObservation(relativePosition.x);
            sensor.AddObservation(relativePosition.z);
        }
    }

    public override void OnEpisodeBegin()
    {
        _visitedSections.Clear();

        // 1. Spawns the Agent and the Player
        _envManager.Begin();

        _lastPosition = transform.localPosition;
        _stuckCounter = 0;

        // 2. NEW: Calculate the starting distance for the Hot and Cold game
        _previousDistance = Vector3.Distance(transform.localPosition, _envManager.Player.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Time penalty
        AddReward(-0.001f);

        // Movement
        float turnInput = actionBuffers.ContinuousActions[0];
        float moveInput = actionBuffers.ContinuousActions[1];

        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        // NEW: THE HOT AND COLD GAME
        float currentDistance = Vector3.Distance(transform.localPosition, _envManager.Player.transform.localPosition);
        float distanceDelta = _previousDistance - currentDistance;

        // Reward for moving closer, penalty for moving away
        AddReward(distanceDelta * 1.0f);

        // Save the distance for the next frame
        _previousDistance = currentDistance;

        CheckIfFallenOver();
        CheckForNewSection();

        // Intentionally commented out to allow spinning without dying!
        // CheckIfStuck(); 

        CheckForTargetGrab();
    }

    private void CheckIfFallenOver()
    {
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private void CheckForTargetGrab()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                AddReward(5.0f);
                EndEpisode();
                return;
            }
        }
    }

    private void CheckForNewSection()
    {
        for (int i = 0; i < _envManager.SectionMiddleCoordinates.Length; i++)
        {
            float dist = Vector3.Distance(transform.localPosition, _envManager.SectionMiddleCoordinates[i]);
            if (dist < _envManager.SectionOffset - 2 && !_visitedSections.Contains(i))
            {
                _visitedSections.Add(i);
                AddReward(0.3f);
            }
        }
    }

    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.localPosition, _lastPosition);
        if (distanceMoved < 0.01f)
        {
            _stuckCounter++;
            if (_stuckCounter > 50)
            {
                AddReward(-1.0f);
                EndEpisode();
            }
        }
        else
        {
            _stuckCounter = 0;
        }
        _lastPosition = transform.localPosition;
    }
    private void OnCollisionStay(Collision collision)
    {
        // Punishes the AI if it grinds against a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.001f);
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