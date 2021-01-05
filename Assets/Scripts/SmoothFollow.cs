﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothFollow : MonoBehaviour
{
    private static SmoothFollow _instance;

    public static SmoothFollow Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public List<Transform> targets;

    public Vector3 offset;
    public float smoothsTime = .5f;

    public float minZoom = 70;
    public float maxZoom = 60;
    public float zoomLimiter = 10;

    private Vector3 velocity;
    private Camera cam;
    public bool isOnFinish;
    public Transform FinishView;

    private void Start()
    {
        cam = GetComponent<Camera>();
        offset = transform.position - targets[0].position;
    }

    private void LateUpdate()
    {
        if (isOnFinish)
        {
            //Debug.Log("on finish!");
            //targets.Clear();
            //transform.position = FinishView.position;
            return;
        }
        if (targets.Count == 0 || !GameManager.Instance.isGameStarted)
            return;

        Move();
        Zoom();
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    private void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        Vector3 newPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothsTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}
