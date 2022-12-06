using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{

    [SerializeField]
    private Vector3 goalPosition;

    [SerializeField]
    private float speed = 0.5f;

    private float current, target;

    // Start is called before the first frame update
    void Start()
    {
        var MyValue = Mathf.Lerp(0, 10, .5f);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0)) target = target == 0 ? 1 : 0;

        current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);

        //new Vector3 = Vector3 Lerp(0, 1, timer);
        //transform.Translate(80 * Time.delta, -80 * Time.deltaTime, 0);

        transform.position = Vector3.Lerp(Vector3.zero, goalPosition, current);
        
    }
}
