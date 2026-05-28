using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class PropHuntAgent : Agent
{
    public float moveSpeed = 5f;
    public float turnSpeed = 150f;
    private Vector3 _lastPosition;
    private int _stuckCounter = 0;

    public StartEpisode _envManager;
    private HashSet<int> _visitedSections = new HashSet<int>();
    private Rigidbody _rb;

    public override void Initialize()
    {
        // Haal de Rigidbody op voor physics-gebaseerde beweging
        _rb = GetComponent<Rigidbody>();

        // Zorg ervoor dat de physics engine de rotatie van de agent niet verstoort
        if (_rb != null)
        {
            _rb.freezeRotation = true;
        }
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
        _envManager.Begin();
        _lastPosition = transform.localPosition;
        _stuckCounter = 0;

        // Reset de snelheid aan het begin van elke nieuwe poging
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 1. Time penalty (prikkel om zo snel mogelijk te zijn)
        AddReward(-0.001f);

        // 2. Beweging via Physics (Rigidbody) in plaats van transform.Translate
        float turnInput = actionBuffers.ContinuousActions[0];
        float moveInput = Mathf.Clamp(actionBuffers.ContinuousActions[1], 0f, 1f);

        // Rotatie kan via transform blijven gaan (werkt perfect i.c.m. freezeRotation)
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        // Pas de snelheid aan met behoud van de verticale snelheid (voor zwaartekracht)
        if (_rb != null)
        {
            Vector3 moveVelocity = transform.forward * moveInput * moveSpeed;
            moveVelocity.y = _rb.linearVelocity.y; // Zorgt ervoor dat hij niet gaat zweven of door de grond zakt
            _rb.linearVelocity = moveVelocity;
        }

        CheckIfFallenOver();
        CheckForNewSection();
        CheckForTargetGrab();

        // 3. Stuck Check (Aangepast: Geen EndEpisode meer!)
        CheckIfStuck(moveInput);
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
                AddReward(5.0f); // Grote hoofdwaarde voor het daadwerkelijk behalen van het doel
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
                AddReward(0.3f); // Beloning voor exploratie van nieuwe kamers
            }
        }
    }

    private void CheckIfStuck(float moveInput)
    {
        float distanceMoved = Vector3.Distance(transform.localPosition, _lastPosition);

        // Hij geeft gas maar komt niet vooruit (staat tegen een muur of object)
        if (moveInput > 0.1f && distanceMoved < 0.01f)
        {
            _stuckCounter++;
            if (_stuckCounter > 50)
            {
                // Geef een milde waarschuwingstraf in plaats van de episode direct te stoppen
                AddReward(-0.05f);
                _stuckCounter = 0; // Reset om constante opeenvolgende straffen te voorkomen
            }
        }
        else
        {
            _stuckCounter = 0;
        }
        _lastPosition = transform.localPosition;
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