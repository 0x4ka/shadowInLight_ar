using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleContoroller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        Color color = renderer.material.color;

        color = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1, 1);
        renderer.material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -800 || transform.position.y > 800 || transform.position.x < -800 || transform.position.x > 800)
        {
            Destroy(gameObject);
        }

        transform.Rotate(0, 0, 0.2f);
    }
}