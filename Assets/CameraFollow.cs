using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform playerTransform;
    public float offset;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    //player movement is handled during FixedUpdate()
    //this is called after Update() and FixedUpdate()
    {
        //store current camera position in variable temp
        Vector3 temp = transform.position;

        //set camera x loc. to player's, then apply offset
        temp.x = playerTransform.position.x;
        temp.x += offset;

        temp.y = playerTransform.position.y;

        //set back campera temp position to be the current position
        transform.position = temp;

    }
}
