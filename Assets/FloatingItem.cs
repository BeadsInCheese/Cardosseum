using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    private Vector3 pos_1;
    private Vector3 pos_2;
    public float incrementY = 5f;
    public float speed = 7f;
    private bool isDirectionToPos1 = false;
    public AnimationCurve ac;
    public bool nonlinear = false;
    // Start is called before the first frame update
    void Start()
    {
        pos_1 = transform.localPosition;
        pos_2 = new Vector3(transform.localPosition.x, transform.localPosition.y + incrementY, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (nonlinear)
        {
            if (isDirectionToPos1)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos_1, speed * ac.Evaluate(((transform.localPosition - pos_1).magnitude / (pos_2 - pos_1).magnitude)) * Time.deltaTime);
                if (transform.localPosition == pos_1)
                    isDirectionToPos1 = false;
            }
            else
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos_2, speed * ac.Evaluate(((transform.localPosition - pos_1).magnitude / (pos_2 - pos_1).magnitude)) * Time.deltaTime);
                if (transform.localPosition == pos_2)
                    isDirectionToPos1 = true;
            }
            return;
        }
        if (isDirectionToPos1)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos_1, speed * Time.deltaTime);
            if (transform.localPosition == pos_1)
                isDirectionToPos1 = false;
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, pos_2, speed  * Time.deltaTime);
            if (transform.localPosition == pos_2)
                isDirectionToPos1 = true;
        }
    }
}
