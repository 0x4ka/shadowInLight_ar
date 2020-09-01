using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShilhouetteController : MonoBehaviour
{
    public GameObject RawImage;
    public GameObject Shilouette;

    public Toggle WebCamOnOff;

    // Start is called before the first frame update
    void Start()
    {
        WebCamOnOff = WebCamOnOff.GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(5, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(-5, 0, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(0, 5, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(0, -5, 0);
        }

        if (Input.GetKey("w"))
        {
            this.gameObject.transform.localScale = new Vector3(
                this.gameObject.transform.localScale.x * 1.01f,
                this.gameObject.transform.localScale.y * 1.01f,
                1);
        }
        if (Input.GetKey("s"))
        {
            this.gameObject.transform.localScale = new Vector3(
                this.gameObject.transform.localScale.x * 0.99f,
                this.gameObject.transform.localScale.y * 0.99f,
                1);
        }

        if (WebCamOnOff.isOn == true)
        {
            RawImage.SetActive(true);
        }
        else
        {
            RawImage.SetActive(false);
        }
    }
}
