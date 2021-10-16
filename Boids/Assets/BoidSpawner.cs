using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    Vector2 screenBounds;
    public int boidQuantity;
    public GameObject boid;
    GameObject[] boids;

    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        for (int i = 1; i <= boidQuantity; i++)
        {
            Vector3 rdmPos = new Vector3(Random.Range(screenBounds.x, screenBounds.x * -1), Random.Range(screenBounds.y, screenBounds.y * -1));
            float angle = Random.Range(0, 2 * Mathf.PI) - Mathf.PI;

            GameObject newBoid = Object.Instantiate(boid, rdmPos, Quaternion.identity);
            newBoid.GetComponent<BoidController>().setAngle(angle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
