using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetController : MonoBehaviour
{
    public Vector3 rotation;
    public Action OnCollected;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime, Space.World);   
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("who");
        if (other.tag == "Player")
            OnCollected();

    }
}
