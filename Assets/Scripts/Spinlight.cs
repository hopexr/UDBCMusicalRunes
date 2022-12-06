using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinlight : MonoBehaviour
{
    [SerializeField]
    private float velocity = 10f;

    [SerializeField]
    private RectTransform spinlight;

    // Update is called once per frame
    void Update()
    {
        spinlight.Rotate(0, 0, Time.deltaTime * velocity);
    }
}
