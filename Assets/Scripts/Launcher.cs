﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Launcher : MonoBehaviour
{
    public Rigidbody rb;
    Vector3 target;

    float h;

    void Start()
    {
        rb.useGravity = false;
        h = Random.Range(8, 10);
    }

    public void Launch(Vector3 tr, float m_moveSpeed)
    {
        target = tr;
        rb.useGravity = true;
        rb.velocity = CalculateLaunchData(m_moveSpeed).initialVelocity;
    }

    LaunchData CalculateLaunchData(float m_moveSpeed)
    {
        float displacementY = target.y - rb.position.y;
        float time = Mathf.Sqrt(-2 * h / Physics.gravity.y) + Mathf.Sqrt(2 * (displacementY - h) / Physics.gravity.y);
        target = new Vector3(target.x, target.y, target.z + (m_moveSpeed * time) + Random.Range(-1f, 1f));


        Vector3 displacementXZ = new Vector3(target.x - rb.position.x, 0, target.z - rb.position.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * Physics.gravity.y * h);
        Vector3 velocityXZ = displacementXZ / time;

        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(Physics.gravity.y), time);
    }

    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

    }
}
