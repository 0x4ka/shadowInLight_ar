using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleGenerator : MonoBehaviour
{
    public GameObject CirclePrefab;
    float span = 5.0f;
    float delta = 0;

    // Update is called once per frame
    void Update()
    {
        this.delta += Time.deltaTime;

        if (this.delta > this.span)
        {
            this.delta = 0;
            this.span = Random.Range(1, 10);

            GameObject go = Instantiate(CirclePrefab) as GameObject;
            int px = Random.Range(-700, 700);
            float sc = Random.Range(150 * 0.8f, 150 * 1.5f);
            go.transform.position = new Vector3(px, 700, 0);

            go.transform.localScale = new Vector3(sc, sc, 1);
        }

        
    }
}
