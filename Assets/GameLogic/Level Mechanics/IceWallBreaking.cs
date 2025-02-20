using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWallBreaking : MonoBehaviour
{
    [SerializeField] private GameObject _replacement;
    [SerializeField] private GameObject _original;
    [SerializeField] private float _breakForce = 2;
    [SerializeField] private float _collisionMultiplier = 100;
    [SerializeField] private bool _broken = false;

    void OnCollisionEnter(Collision collision)
    {
        if (_broken) return;

        if (collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player1")
        {
            if (collision.gameObject.tag == "Player2")
            {
                PlayerController movement = collision.gameObject.GetComponent<PlayerController>();
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();  
                if (movement != null)
                {
                    Debug.Log("STOP!");
                    movement.is_sliding = false;
                    movement.startMoving = false;
                    movement.PlayerSetBack();
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
            }
            else if (collision.gameObject.tag == "Player1")
            {
                PlayerController movement = collision.gameObject.GetComponent<PlayerController>();
                Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
                if (movement != null)
                {
                    Debug.Log("STOP!");
                    movement.is_sliding = false;
                    movement.startMoving = false;
                    movement.PlayerSetBack();
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
            }
            _broken = true;
            var replacement = Instantiate(_replacement, transform.position, transform.rotation);

            var rbs = replacement.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionMultiplier, collision.contacts[0].point, 2);
            }
            Destroy(_original);
            Destroy(gameObject);
        }

    }
}
