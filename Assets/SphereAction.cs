using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class SphereAction : Agent
{
    Rigidbody2D sphere;
    public float speed;
    ParticleSystem ps;

    ParticleSystem.Particle[] pp;
    int collisionCount;

    public override void Initialize()
    {
        sphere = GetComponent<Rigidbody2D>();
        speed = 10.0f;
        ps = Component.FindObjectOfType<ParticleSystem>();
        var main = ps.main;
        pp = new ParticleSystem.Particle[main.maxParticles];
    }

    public override void OnEpisodeBegin()
    {
        if (this.transform.localPosition.y != 0) {
            this.transform.localPosition = new Vector3(0, 0, 0);
        }
        ps.Clear();
        ps.Play();
        collisionCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(ps.transform.localPosition);

        sensor.AddObservation(sphere.velocity.x);
        var main = ps.main;
        sensor.AddObservation(main.simulationSpeed);
    }

    void OnParticleCollision(){
        collisionCount = collisionCount + 1;

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        sphere.AddForce(controlSignal * speed);


        if (collisionCount > 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        else if (this.transform.localPosition.y < -4)
        {
            EndEpisode();
        }

        int numParticlesAlive = ps.GetParticles(pp);
        for (int i=0; i<numParticlesAlive; i++) {
            if ((pp[i].position.y < -2) && (Math.Abs(pp[i].position.x - this.transform.localPosition.x) > 2) ) {
                SetReward(0.1f);
            }
        }
        

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
    }



}
