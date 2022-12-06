using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WigSolution : MonoBehaviour
{

    public float moveSpeed = 10f;
    public float distance = 2f;
    public float time = 0f;

    private float startingPositionX;
    private float startingPositionY;
    

    // Start is called before the first frame update
    void Start()
    {
        startingPositionX = transform.position.x;
        Utilities.PrintMessage("Hello world!");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.x = startingPositionX + Mathf.Sin(Time.time * moveSpeed) * distance;
        transform.position = Vector3.Lerp(transform.position, currentPosition, time * Time.deltaTime);
        Color.Lerp(Color.red, Color.blue, time * Time.deltaTime);

        // Steve's solution: transform.position = Vector3.Lerp(pointA, pointB, Mathf.PingPong(Time.time * moveSpeed, 1f));

        // //Oscillate between two points
        // Vector3 add = Vector3.Lerp(pointOne, pointTwo, (Mathf.Sin(Time.time) + 1) / 2); 
        //Oscillate between colors cubeMaterial.color = Color.Lerp(Color.red, Color.blue, (Mathf.Sin(Time.time) + 1)/2); 
        //Set position transform.position = start + add;

    }
}
