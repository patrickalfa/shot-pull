using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DistanceJoint2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration;
    public float maxSpeed;
    public float jumpForce;
    public LayerMask groundMask;
    public Vector2[] repelDirections;

    [Header("Force")]
    public float shootForce;
    public float rappelForce;
    public float pullForce;

    /////////////////////////////////////////

    private float stringForce;
    private bool charging;
    private bool grounded;

    /////////////////////////////////////////

    public Arrow _arrow;
    public Bow _bow;
    public Rope _rope;

    private Transform _trf;
    private Rigidbody2D _rgbd;
    private DistanceJoint2D _joint;

    /////////////////////////////////////////

    [Header("DEBUG")]
    public float maxVelocity;
    public bool holding;
    private bool __isLoaded;
    public bool isLoaded
    {
        get
        {
            return __isLoaded;
        }
        set
        {
            __isLoaded = value;
            _rope.active = !value;
        }
    }

    private void Start()
    {
        _trf = transform;
        _rgbd = GetComponent<Rigidbody2D>();
        _joint = GetComponent<DistanceJoint2D>();
        isLoaded = true;
    }

    private void Update()
    {
        UpdateArrow();
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(_trf.position + (Vector3.down * 1f), .5f, groundMask);
        Debug.DrawLine(_trf.position, _trf.position + (Vector3.down * 1.5f), (grounded ? Color.cyan : Color.magenta));

        LimitVelocity();
    }

    /////////////////////////////////////////

    private void LimitVelocity()
    {
        Vector2 vel = _rgbd.velocity;
        float curMaxVelocity = (charging ? maxVelocity * .25f : maxVelocity);

        if (vel.magnitude > curMaxVelocity)
            _rgbd.velocity = vel.normalized * curMaxVelocity;
    }

    /////////////////////////////////////////

    public void Move(float direction)
    {
        if (charging)
            direction = 0f;

        _rgbd.AddForce(Vector2.right * direction * acceleration * Time.deltaTime);

        Vector2 vel = _rgbd.velocity;
        if (Mathf.Abs(vel.x) > maxSpeed)
            vel.x = (vel.x / Mathf.Abs(vel.x)) * maxSpeed;

        _rgbd.velocity = vel;
    }

    public void Jump()
    {
        if (!grounded)
            return;

        Vector2 vel = _rgbd.velocity;
        vel.y = 0f;
        _rgbd.velocity = vel;

        _rgbd.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void ChargeShot()
    {
        if (!isLoaded)
            return;

        charging = true;

        stringForce = Mathf.Clamp(stringForce + Time.deltaTime, 0f, 1f);
        _bow.UpdateString(stringForce);
    }

    public void Shoot(Vector2 axis)
    {
        if (!isLoaded)
            return;

        charging = false;

        isLoaded = false;
        _arrow.Deploy(axis * (shootForce * stringForce));

        stringForce = 0f;
        _bow.UpdateString(stringForce);
    }

    public void Pull()
    {
        if (isLoaded)
            return;

        if (!_arrow.deployed)
        {
            _rope.returning = true;
            _arrow.active = false;
            _arrow.SetDeployed(true);
        }

        _arrow.Pull(_rope.GetLastPoint(), pullForce);
    }

    public void StopPulling()
    {
        _rope.returning = false;
        _arrow.active = true;
    }

    public void Rappel()
    {
        if (isLoaded || _arrow.deployed)
            return;

        Vector2 ropePoint = _rope.GetFirstPoint();

        _rgbd.velocity += (ropePoint - (Vector2)_trf.position).normalized * rappelForce * Time.deltaTime;

        float arrowDistance = Vector2.Distance(ropePoint, _arrow.transform.position);
        float pointDistance = Vector2.Distance(_trf.position, ropePoint);

        if (arrowDistance > 1f && pointDistance < 1.75f)
        {
            foreach (Vector2 dir in repelDirections)
                RepelWall(dir);
        }
    }

    public void HoldRope()
    {
        if (isLoaded || _arrow.deployed || _joint.enabled ||
            _trf.position.y >= _rope.GetFirstPoint().y)
            return;

        _joint.enabled = true;

        holding = true;
    }

    public void RestrainRope()
    {
        if (isLoaded || !_arrow.deployed || _joint.enabled)
            return;

        _rope.Restrain();
    }

    public void ReleaseRope()
    {
        if (_joint.enabled)
            _joint.enabled = false;

        if (!holding)
            _rope.Release();

        holding = false;
    }

    public void SetAxis(Vector2 axis)
    {
        _bow.transform.right = axis;

        if (isLoaded)
        {
            _arrow.transform.up = axis;
        }
    }

    private void UpdateArrow()
    {
        if (isLoaded)
        {
            Vector3 arrowPos = _trf.position +
                (_arrow.transform.up * (1.3f - (stringForce * .75f)));
                
            _arrow.transform.position = arrowPos;
        }
        else
        {
            if (_joint.enabled && _trf.position.y >= _rope.GetFirstPoint().y)
                ReleaseRope();
        }
    }

    private void RepelWall(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(_trf.position, dir, 1.75f, LayerMask.GetMask("Solid"));
        if (hit)
        {
            _rgbd.velocity += -dir * rappelForce * Time.deltaTime;
            Debug.DrawLine(_trf.position, (Vector2)_trf.position + (dir * 1.75f), Color.black);
        }
        else
            Debug.DrawLine(_trf.position, (Vector2)_trf.position + (dir * 1.75f), Color.white);
    }
}