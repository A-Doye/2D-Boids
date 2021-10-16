using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static System.Math;

public class DebuggerBoid : MonoBehaviour
{
    float x = 0, y = 0, z = 0;

    SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoidController>().showLos(true);

        //GetComponent<BoidController>().setAngle(1f*Mathf.PI);

        sr = GetComponent<SpriteRenderer>();

        sr.color = Color.red;

        if (Mathf.DeltaAngle(0, -90) < 0)
        {
            Debug.Log("Here!" + Mathf.DeltaAngle(0, -90));
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<BoidController>().showLos(true);
    }

    private float averageAngle(float[] angles)
    {
        var x = angles.Sum(a => Cos(a * PI / 180)) / angles.Length;
        var y = angles.Sum(a => Sin(a * PI / 180)) / angles.Length;
        return (float)Atan2(y, x);
    }
}
