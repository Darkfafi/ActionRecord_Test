using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LauchAwayFromClick : MonoBehaviour {

    [SerializeField]
    private KeyCode keyToTrigger = KeyCode.E;

	protected void Update()
    {
        if(Input.GetKeyDown(keyToTrigger))
        {
            GetComponent<Rigidbody2D>().AddForce((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position) * 100);
        }
    }
}
