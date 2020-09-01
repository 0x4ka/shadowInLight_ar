using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CircleController : MonoBehaviour
{
    public GameObject CirclePrefab;
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;
    public AudioClip sound4;
    public AudioClip sound5;
    /*public AudioClip sound6;
    public AudioClip sound7;
    public AudioClip sound8;*/

    AudioSource audioSource;

    int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Renderer renderer = this.GetComponent<Renderer>();
        Color color = renderer.material.color;

        color = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1, 1);
        renderer.material.color = color;
    }


    // Update is called once per frame
    void Update()
    {
        //transform.Translate(0, -2.5f, 0);

        if (transform.position.y < -800 || transform.position.y > 800 || transform.position.x < -800 || transform.position.x > 800)
        {
            Destroy(gameObject);
        }

    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Renderer renderer = this.GetComponent<Renderer>();
        Color color = renderer.material.color;

        color = Color.HSVToRGB(Random.Range(0.0f, 1.0f), 1, 1);
        renderer.material.color = color;

        i = Random.Range(1, 6);

        if (i == 1) audioSource.PlayOneShot(sound1);
        if (i == 2) audioSource.PlayOneShot(sound2);
        if (i == 3) audioSource.PlayOneShot(sound3);
        if (i == 4) audioSource.PlayOneShot(sound4);
        if (i == 5) audioSource.PlayOneShot(sound5);
        /*if (i == 6) audioSource.PlayOneShot(sound6);
        if (i == 7) audioSource.PlayOneShot(sound7);
        if (i == 8) audioSource.PlayOneShot(sound8);*/


        /*var mat = this.GetComponent<Rigidbody2D>().sharedMaterial;
        if (Input.GetKey("x"))
        {
            mat.bounciness += 0.1f;
        }
        if (Input.GetKey("z"))
        {
            mat.bounciness -= 0.1f;
        }
        */
    }
}