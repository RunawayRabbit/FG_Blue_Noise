
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private float movespeed = 10.0f;

    // Update is called once per frame
    void Update()
    {
        var delta = new Vector3();

        if( Input.GetKey( KeyCode.W ) )
        {
            delta += Vector3.forward;
        }
        if( Input.GetKey( KeyCode.S ) )
        {
            delta += Vector3.back;
        }

        if( Input.GetKey( KeyCode.A ) )
        {
            delta += Vector3.left;
        }
        if( Input.GetKey( KeyCode.D ) )
        {
            delta += Vector3.right;
        }

        transform.position += delta * movespeed * Time.deltaTime;
    }
}
