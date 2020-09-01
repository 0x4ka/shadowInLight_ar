using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{
    public GameObject Canvasdebug;
    int i = 0;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (i == 0)
            {
                Canvasdebug.SetActive(true);
                i = 1;
            }
            else
            {
                Canvasdebug.SetActive(false);
                i = 0;
            }
        }
    }


}