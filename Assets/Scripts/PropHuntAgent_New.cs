using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class PropHuntAgent_New : Agent
{
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    private Rigidbody _rb;

    private Animator _animator;

    [Header("Game Connecties")]
    public HideAndSeekManager gameManager;

    [Header("Map Settings")]
    public Vector3[] SectionMiddleCoordinates;
    public float SectionOffset;

    [Header("Spawn Settings")]
    public Vector3 centerSpawnPosition = new Vector3(0, 1f, 0);


    private HashSet<int> _visitedSections = new HashSet<int>();

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.freezeRotation = true;
        }
        _animator = GetComponentInChildren<Animator>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = centerSpawnPosition;
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        _visitedSections.Clear();

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_visitedSections.Count);

        for (int i = 0; i < SectionMiddleCoordinates.Length; i++)
        {
            sensor.AddObservation(_visitedSections.Contains(i) ? 1.0f : 0.0f);
            Vector3 relativePosition = transform.InverseTransformPoint(SectionMiddleCoordinates[i]);
            sensor.AddObservation(relativePosition.x);
            sensor.AddObservation(relativePosition.z);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (gameManager == null || !gameManager.aiBrain.enabled)
        {
            if (_rb != null) _rb.linearVelocity = Vector3.zero;
            if (_animator != null) _animator.SetFloat("Snelheid", 0f);
            return;
        }

        float turnInput = actionBuffers.ContinuousActions[0];
        float moveInput = Mathf.Clamp(actionBuffers.ContinuousActions[1], 0f, 1f);

        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.fixedDeltaTime);

        if (_rb != null)
        {
            Vector3 moveVelocity = transform.forward * moveInput * moveSpeed;
            moveVelocity.y = _rb.linearVelocity.y;
            _rb.linearVelocity = moveVelocity;
        }

        if (_animator != null)
        {
            _animator.SetFloat("Snelheid", moveInput);
        }

        CheckForNewSection();
        CheckForTargetGrab();
    }

    private void CheckForTargetGrab()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                gameManager.PlayerFound();
                if (_animator != null)
                {
                    _animator.SetTrigger("Dansen");
                    Debug.Log("AI heeft je gepakt en begint te feesten!");
                }
                return;
            }
        }
    }

    private void CheckForNewSection()
    {
        for (int i = 0; i < SectionMiddleCoordinates.Length; i++)
        {
            float dist = Vector3.Distance(transform.localPosition, SectionMiddleCoordinates[i]);
            if (dist < SectionOffset - 2 && !_visitedSections.Contains(i))
            {
                _visitedSections.Add(i);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}