using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheakPoints : MonoBehaviour {
    public Transform a, b, c, d;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            InorOut(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
	}
    void InorOut(Vector3 point)
    {
        if (point.x > a.position.x && point.x < d.position.x && point.y >a.position.y&& point.y < b.position.y)
        {
            print("当前点（" + point + "),在cube中。");
        }
        else
        {
            print("当前点（" + point + "),不在cube中。");
        }
    }
}
