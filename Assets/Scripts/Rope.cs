using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    private bool __active;
    public bool active
    {
        get
        {
            return __active;
        }
        set
        {
            __active = value;
            if (!value)
                ResetRope();
        }
    }

    public bool returning;

    public LayerMask layerMask;
    public Transform _arrowAttach;
    public Transform _hingePlayer;
    public Transform _hingeArrow;

    private Transform _trf;
    private LineRenderer _line;
    private List<Vector2> _points;
    private DistanceJoint2D _arrowJoint;

    private void Awake()
    {
        _trf = transform;
        _line = GetComponent<LineRenderer>();
        _points = new List<Vector2>();
        _arrowJoint = _hingeArrow.GetComponent<DistanceJoint2D>();
    }

    private void FixedUpdate()
    {
        if (!active)
            return;

        CheckWrapPoints();
        SetLinePoints();
        SetHingePosition();
    }

    private void CheckWrapPoints()
    {
        if (returning)
            return;

        CheckLastWrapPoint();
        if (_points.Count > 0)
        {
            CheckFirstWrapPoint();
            CheckFirstUnwrapPoint();

            if (_points.Count > 1)
                CheckLastUnwrapPoint();
        }
    }

    private void CheckLastWrapPoint()
    {
        Vector2 lastPoint = GetLastPoint();

        Vector2 direction = (lastPoint - (Vector2)_arrowAttach.position).normalized;
        float distance = Vector2.Distance(_arrowAttach.position, lastPoint);

        // -- DEBUG ---------------------------------
        Debug.DrawLine(_arrowAttach.position, (Vector2)_arrowAttach.position + (direction * (distance - .1f)), Color.blue);
        // -- DEBUG ---------------------------------

        RaycastHit2D hit = Physics2D.Raycast(_arrowAttach.position, direction, distance - .1f, layerMask);
        if (hit)
            _points.Add(GetClosestPoint(hit, true));
    }

    private void CheckFirstWrapPoint()
    {
        Vector2 firstPoint = GetFirstPoint();

        Vector2 direction = (firstPoint - (Vector2)_trf.position).normalized;
        float distance = Vector2.Distance(_trf.position, firstPoint);

        // -- DEBUG ---------------------------------
        Debug.DrawLine(_trf.position, (Vector2)_trf.position + (direction * (distance - .1f)), Color.green);
        // -- DEBUG ---------------------------------

        RaycastHit2D hit = Physics2D.Raycast(_trf.position, direction, distance - .1f, layerMask);
        if (hit)
            _points.Insert(0, GetClosestPoint(hit, false));
    }

    private void CheckLastUnwrapPoint()
    {
        Vector2 nextPoint = (_points.Count > 1 ? _points[_points.Count - 2] : (Vector2)_trf.position);

        Vector2 direction = (nextPoint - (Vector2)_arrowAttach.position).normalized;
        float distance = Vector2.Distance(_arrowAttach.position, nextPoint);

        // -- DEBUG ---------------------------------
        Debug.DrawLine(_arrowAttach.position, (Vector2)_arrowAttach.position + (direction * (distance - .1f)), Color.red);
        // -- DEBUG ---------------------------------

        RaycastHit2D hit = Physics2D.Raycast(_arrowAttach.position, direction, distance - .1f, layerMask);
        if (!hit && Utils.PointOnLine2D(_points[_points.Count - 2], _arrowAttach.position, nextPoint, 10f))
            _points.RemoveAt(_points.Count - 1);
    }

    private void CheckFirstUnwrapPoint()
    {
        Vector2 nextPoint = (_points.Count > 1 ? _points[1] : (Vector2)_arrowAttach.position);

        Vector2 direction = (nextPoint - (Vector2)_trf.position).normalized;
        float distance = Vector2.Distance(_trf.position, nextPoint);

        // -- DEBUG ---------------------------------
        Debug.DrawLine(_trf.position, (Vector2)_trf.position + (direction * (distance - .1f)), Color.red);
        // -- DEBUG ---------------------------------

        RaycastHit2D hit = Physics2D.Raycast(_trf.position, direction, distance - .1f, layerMask);
        if (!hit && Utils.PointOnLine2D(_points[0], _trf.position, nextPoint, 10f))
            _points.RemoveAt(0);
    }

    private void SetHingePosition()
    {
        if (_points.Count > 0)
        {
            _hingePlayer.position = _points[0];
            _hingeArrow.position = _points[_points.Count - 1];
        }
        else
        {
            _hingePlayer.position = _arrowAttach.position;
            _hingeArrow.position = _trf.position;
        }
    }

    private Vector2 GetClosestPoint(RaycastHit2D hit, bool last)
    {
        PolygonCollider2D col = hit.collider as PolygonCollider2D;

        Vector2 point = Vector2.zero;
        float distance = 999f;
        foreach (Vector2 p in col.points)
        {
            Vector2 wp = col.transform.TransformPoint(p);
            float d = Vector2.Distance(wp, hit.point);

            if (d < distance && IsPointValid(wp, last))
            {
                point = wp;
                distance = d;
            }
        }

        return point;
    }

    private bool IsPointValid(Vector2 point, bool last)
    {
        return !(_points.Count > 0 &&
            ((last && _points[_points.Count - 1].Equals(point)) ||
            (!last && _points[0].Equals(point))));
    }

    private void SetLinePoints()
    {
        _line.positionCount = 2 + _points.Count;
        
        _line.SetPosition(0, _trf.position);
        for (int i = 0; i < _points.Count; i++)
            _line.SetPosition(i + 1, _points[i]);
        _line.SetPosition(_points.Count + 1, _arrowAttach.transform.position);
    }

    //////////////////////////////////////////////////////////

    public void ResetRope()
    {
        _line.positionCount = 0;
        _points.Clear();
        returning = false;
        _arrowJoint.enabled = false;
    }

    public Vector2 GetFirstPoint()
    {
        return (_points.Count > 0) ? _points[0] : (Vector2)_arrowAttach.position;
    }

    public Vector2 GetLastPoint()
    {
        return (_points.Count > 0) ? _points[_points.Count - 1] : (Vector2)_trf.position;
    }

    public void Restrain()
    {
        _arrowJoint.enabled = true;
    }

    public void Release()
    {
        _arrowJoint.enabled = false;
    }

    public void ReturnToPlayer()
    {
        Time.timeScale = .1f;
        returning = true;
        StartCoroutine(WaitForArrowToReturn());
    }

    private IEnumerator WaitForArrowToReturn()
    {
        while (_points.Count > 0)
        {
            _arrowAttach.parent.DOMove(_points[_points.Count - 1], .02f).SetEase(Ease.Linear);

            yield return new WaitForSeconds(.025f);

           _points.RemoveAt(_points.Count - 1);
        }

        _arrowAttach.parent.DOMove(_trf.position, .02f);

        yield return new WaitForSeconds(.02f);

        active = false;

        Time.timeScale = 1f;
    }
}
