using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BezierRope : MonoBehaviour
{
    public int nodeCount;
    public float linkDistance;
    public float speed;
    public float gravity;

    private Transform[] _nodes;
    private LineRenderer _line;

    private void Start()
    {
        _line = GetComponent<LineRenderer>();
        
        CreateNodes();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < nodeCount - 1; i++)
        {
            Vector3 targetPos = _nodes[i + 1].position;
            targetPos += (_nodes[i].position - _nodes[i + 1].position).normalized * linkDistance;
            targetPos += Vector3.down * gravity * Time.deltaTime;

            _nodes[i].position = Vector3.Lerp(_nodes[i].position, targetPos, speed * Time.deltaTime);
        }

        _line.SetPositions(Array.ConvertAll(_nodes, p => p.position));
    }

    private void CreateNodes()
    {
        _nodes = new Transform[nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            GameObject node = new GameObject("Node");
            node.transform.parent = transform;
            node.transform.position = Vector3.right * i * linkDistance;

            _nodes[i] = node.transform;
        }

        _line.positionCount = nodeCount;
    }
}
