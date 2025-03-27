using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField]
    private Transform position1, position2;
    public float _speed = 3.0f;
    private bool _switch = false;


    void FixedUpdate()
    {

        if (_switch == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, position1.position,
                _speed * Time.deltaTime);
        }
        else if (_switch == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, position2.position,
                _speed * Time.deltaTime);
        }

        if (transform.position == position1.position)
        {
            _switch = true;
        }
        else if (transform.position == position2.position)
        {
            _switch = false;
        }
}