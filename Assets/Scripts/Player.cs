using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public float acceleration;
    public float maxSpeed;
    public float jumpForce;
    public float shootForce;
    public float grappleForce;
    public float pullForce;

    public Arrow _arrow;
    public LineRenderer _line;
    private Rigidbody2D _rgbd;
    private Transform _trf;

    [Header("DEBUG")]
    public bool isLoaded = true;
    public float maxVelocity;

    private void Start()
    {
        _rgbd = GetComponent<Rigidbody2D>();
        _trf = transform;
    }

    private void Update()
    {
        UpdateArrow();
        LimitVelocity();
    }

    private void LimitVelocity()
    {
        Vector2 vel = _rgbd.velocity;
        if (vel.magnitude > maxVelocity)
            _rgbd.velocity = vel.normalized * maxVelocity;
    }

    public void Move(float direction)
    {
        _rgbd.AddForce(Vector2.right * direction * acceleration);

        Vector2 vel = _rgbd.velocity;
        if (Mathf.Abs(vel.x) > maxSpeed)
            vel.x = (vel.x / Mathf.Abs(vel.x)) * maxSpeed;

        _rgbd.velocity = vel;
    }

    public void Jump()
    {
        Vector2 vel = _rgbd.velocity;
        vel.y = 0f;
        _rgbd.velocity = vel;

        _rgbd.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void Shoot(Vector2 axis)
    {
        if (!isLoaded)
            return;

        isLoaded = false;
        _arrow.Deploy(axis * shootForce);
    }

    public void Pull()
    {
        if (isLoaded)
            return;

        if (!_arrow.deployed)
        {
            _rgbd.velocity += (Vector2)(_arrow.transform.position - _trf.position).normalized * grappleForce * Time.timeScale;
        }
        else
        {
            _arrow.Pull(_trf.position, pullForce);
        }
    }

    public void SetAxis(Vector2 axis)
    {
        if (isLoaded)
            _arrow.transform.up = axis;
    }

    private void UpdateArrow()
    {
        if (isLoaded)
            _arrow.transform.position = _trf.position + (_arrow.transform.up * .75f);

        Vector3 attachedPos = _arrow.transform.position - (_arrow.transform.up * .5f);
        _line.SetPosition(0, _trf.position);
        _line.SetPosition(1, attachedPos);
    }
}