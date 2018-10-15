using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMController : MonoBehaviour
{
    public Player _player;

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        _player.Move(Input.GetAxisRaw("Horizontal"));

        ////////////////////////////////////////////

        if (Input.GetButtonDown("Jump"))
            _player.Jump();

        ////////////////////////////////////////////

        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = _cam.ScreenToWorldPoint(mousePos);
        screenPos.z = 0f;
        Vector3 axis = (screenPos - _player.transform.position).normalized;

        ////////////////////////////////////////////

        _player.SetAxis(axis);

        ////////////////////////////////////////////

        if (Input.GetButton("Fire1"))
            _player.ChargeShot();

        if (Input.GetButtonUp("Fire1"))
            _player.Shoot(axis);

        ////////////////////////////////////////////

        if (Input.GetButton("Fire2"))
            _player.Pull();

        ////////////////////////////////////////////

        if (Input.GetButton("Fire3"))
            _player.HoldRope();

        if (Input.GetButtonUp("Fire3"))
            _player.ReleaseRope();
    }
}
