using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleGenerator : MonoBehaviour
{
    public GameObject TrianglePrefab;
    float span = 5.0f;
    float delta = 0;

    // Update is called once per frame
    void Update()
    {
        this.delta += Time.deltaTime;

        if (this.delta > this.span)
        {
            this.delta = 0;
            this.span = Random.Range(10, 20);

            GameObject go = Instantiate(TrianglePrefab) as GameObject;
            int px = Random.Range(-700, 700);
            float sc = Random.Range(800 * 0.8f, 800 * 1.5f);
            go.transform.position = new Vector3(px, 700, 0);

            go.transform.localScale = new Vector3(sc, sc, 1);
        }
    }
}
