using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanAfterPlaying : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject,3);
    }

    // Update is called once per frame

}