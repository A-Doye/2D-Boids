using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static System.Math;

// To fix / improve
// Colour changes for debug.ray intensity
// Multiple rays hitting the same target

public class BoidController : MonoBehaviour
{
    public float speed;
    float turnSpeed = 24f;
    float angle;
    float targetAngle;
    Vector2 angleVec;
    float lineOfSight = 0.5f;
    bool debugLos;
    float fov = 2f * Mathf.PI; //0.75f * Mathf.PI;
    Vector3 pos;

    

    Vector2 screenBounds;

    // Start is called before the first frame update
    void Start()
    {
        speed = 4f;
        targetAngle = angle;

        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        targetAngle = Mathf.Clamp(targetAngle, angle - (2 * (Mathf.PI/90)), angle + (2 * (Mathf.PI/90)));
        restrictAngle(targetAngle);
        angle = targetAngle;

        transform.position += new Vector3(lengthdir_x(speed, targetAngle), lengthdir_y(speed, targetAngle)) * Time.deltaTime;

        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, new Vector2 (Mathf.Cos(targetAngle), -Mathf.Sin(targetAngle)));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 25600f);
    }

    void FixedUpdate()
    {
        float sep = separationFunc();
        float aln = alignmentFunc();

        targetAngle += sep + aln;

        Debug.DrawRay(transform.position, losVec(targetAngle, lineOfSight), Color.red);
    }

    void LateUpdate()
    {
        ScreenWrap();
    }

    // --- Boid controlling functions ---
    // Separation (the desire to not get too close to one another)
    private float separationFunc()
    {
        float angleChange = 0;

        int separationSegmentQuantity = 12;
        float separationSegmentSize = fov / (separationSegmentQuantity + 1);
        float currentAngle = targetAngle - (fov / 2);

        List<GameObject> separationBoids = new List<GameObject>();

        for (int i = 1; i <= separationSegmentQuantity; i++)
        {
            currentAngle += separationSegmentSize;
            restrictAngle(currentAngle);
            RaycastHit2D separationHit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(currentAngle), -Mathf.Sin(currentAngle)), lineOfSight);
            if (separationHit.collider != null)
            {
                float turnIntensity = separationHit.fraction;
                Color c = new Color(1 - (turnIntensity * 1), 1f, 0f);

                Transform objectHit = separationHit.transform;
                if (!separationBoids.Contains(objectHit.gameObject))
                {
                    separationBoids.Add(objectHit.gameObject);
                }
            
                if (debugLos) Debug.DrawRay(transform.position, losVec(currentAngle, lineOfSight), c);
            }
            else
            {
                if (debugLos) Debug.DrawRay(transform.position, losVec(currentAngle, lineOfSight), Color.white);
            }
        }

        for (int i = separationBoids.Count - 1; i >= 0; i--)
        {
            float turnIntensity = (Vector2.Distance(transform.position, separationBoids[i].transform.position) + 0.25f) / lineOfSight;

            float dir = Vector2.SignedAngle(transform.position, separationBoids[i].transform.position);

            if (dir > targetAngle-Mathf.PI)
            {
                angleChange += turnSpeed * turnIntensity * Time.deltaTime;
            }
            else
            {
                angleChange -= turnSpeed * turnIntensity * Time.deltaTime;
            }
        }
        //Debug.Log(angleChange);
        return angleChange;
    }


    // Alignment (the desire to go in the same direction as others)
    private float alignmentFunc()
    {
        float angleChange = 0;

        int alignmentSegmentQuantity = 24;
        float alignmentSegmentSize = (2 * Mathf.PI) / (alignmentSegmentQuantity);
        float currentAngle = angle;
        List<float> alignmentAngle = new List<float>();
        alignmentAngle.Add(angle * (180 / Mathf.PI));

        List<GameObject> alignmentBoids = new List<GameObject>();

        for (int i = 1; i <= alignmentSegmentQuantity; i++)
        {
            currentAngle += alignmentSegmentSize;
            RaycastHit2D alignmentHit = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(currentAngle), -Mathf.Sin(currentAngle)), 2f * lineOfSight);
            //Debug.DrawRay(transform.position, losVec(currentAngle, 2f*lineOfSight), Color.white);
            if (alignmentHit.collider != null)
            {
                Transform objectHit = alignmentHit.transform;
                if (!alignmentBoids.Contains(objectHit.gameObject))
                {
                    alignmentBoids.Add(objectHit.gameObject);
                }
            }
        }

        for (int i = alignmentBoids.Count - 1; i >= 0; i--)
        {
            /*
            if (Vector2.Distance(transform.position, alignmentBoids[i].transform.position) > 2f * lineOfSight)
            {
                alignmentBoids.Remove(alignmentBoids[i]);
                continue;
            }
            */
            alignmentAngle.Add(alignmentBoids[i].gameObject.GetComponent<BoidController>().getTargetAngle() * (180 / Mathf.PI));
        }

        angleChange = averageAngle(alignmentAngle.ToArray());

        float temp = Mathf.DeltaAngle(Mathf.Rad2Deg * (targetAngle - Mathf.PI), Mathf.Rad2Deg * angleChange);
            

        if (temp < 0)
        {
            angleChange = 0.05f;
        }
        else if (temp > 0)
        {
            angleChange = -0.05f;
        }
        else
        {
            angleChange = 0f;
        }

        angleChange = Mathf.Clamp(angleChange, -0.5f, 0.5f);

        return angleChange;
    }

    // Restricts an angle to >= 0 && <= 2PI
    private float restrictAngle(float angle)
    {
        while (angle > 2 * Mathf.PI)
        {
            angle -= 2 * Mathf.PI;
        }
        while (angle < 0)
        {
            angle += 2 * Mathf.PI;
        }

        return angle;
    }

    // Converts radian angle to 2D vector angle
    private Vector2 losVec(float angle, float lineOfSight)
    {
        Vector2 losVec = new Vector2(lengthdir_x(lineOfSight, angle), lengthdir_y(lineOfSight, angle));
        return losVec;
    }

    // Returns X movement based on movement angle and speed
    private float lengthdir_x(float len, float dir)
    {
        return Mathf.Cos(dir) * len;
    }
    // Returns Y movement based on movement angle and speed
    private float lengthdir_y(float len, float dir)
    {
        return -Mathf.Sin(dir) * len;
    }

    // Restricts objects to the screen
    private void ScreenWrap()
    {
        Vector3 viewPos = transform.position;

        viewPos.x = Mathf.Clamp(viewPos.x, screenBounds.x, screenBounds.x * -1);
        viewPos.y = Mathf.Clamp(viewPos.y, screenBounds.y, screenBounds.y * -1);
        
        if (viewPos.x == screenBounds.x)
        {
            viewPos.x = screenBounds.x * -1 -0.000001f;
            //viewPos.y = Mathf.Abs(screenBounds.y) * Mathf.Sin(angle);
            //Debug.Log("Left");
        }
        if (viewPos.x == screenBounds.x * -1)
        {
            viewPos.x = screenBounds.x;
            //viewPos.y = Mathf.Abs(screenBounds.y) * Mathf.Sin(angle);
            //Debug.Log("Right");
        }
        if (viewPos.y == screenBounds.y)
        {
            //viewPos.x = Mathf.Abs(screenBounds.x) * -Mathf.Cos(angle);
            viewPos.y = screenBounds.y * -1 -0.000001f;
            //Debug.Log("Bottom");
        }
        if (viewPos.y == screenBounds.y * -1)
        {
            //viewPos.x = Mathf.Abs(screenBounds.x) * -Mathf.Cos(angle);
            viewPos.y = screenBounds.y;
            //Debug.Log("Top");
        }

        transform.position = viewPos;
    }

    private float averageAngle(float[] angles)
    {
        var x = angles.Sum(a => Cos(a * PI / 180)) / angles.Length;
        var y = angles.Sum(a => Sin(a * PI / 180)) / angles.Length;
        return (float)Atan2(y, x);
    }

    // Mutators
    public float getAngle()
    {
        return angle;
    }

    public float getTargetAngle()
    {
        return targetAngle;
    }

    public void setAngle(float angle)
    {
        this.angle = angle;
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    // Debugging functions
    public void showLos(bool showLos)
    {
        debugLos = showLos;
    }
}