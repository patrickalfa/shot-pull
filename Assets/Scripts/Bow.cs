using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    private LineRenderer _string;

	private void Start()
    {
        _string = transform.Find("String").GetComponent<LineRenderer>();
	}

    public void UpdateString(float force)
    {
        _string.SetPosition(1, Vector3.right * -force * .75f);
    }
}
