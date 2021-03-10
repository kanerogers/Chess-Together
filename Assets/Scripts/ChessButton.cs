using UnityEngine;
using UnityEngine.Events;

public class ChessButton : MonoBehaviour
{
    [System.Serializable]
    public class ButtonEvent : UnityEvent { }

    public float pressLength;
    public bool pressed;
    public ButtonEvent downEvent;

    Vector3 startPos;
    Rigidbody rb;

    void Start()
    {
        startPos = transform.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // If our distance is greater than what we specified as a press
        // set it to our max distance and register a press if we haven't already
        float distance = Mathf.Abs(transform.localPosition.y - startPos.y);
        if (distance >= pressLength)
        {
            // Prevent the button from going past the pressLength
            transform.localPosition = new Vector3(transform.localPosition.x, startPos.y - pressLength, transform.localPosition.z);
            if (!pressed)
            {
                pressed = true;
                // If we have an event, invoke it
                downEvent?.Invoke();
            }
        }
        else
        {
            // If we aren't all the way down, reset our press
            pressed = false;
        }
        // Prevent button from springing back up past its original localPosition
        if (transform.localPosition.x != startPos.x)
        {
            transform.localPosition = new Vector3(startPos.x, transform.localPosition.y, startPos.z);
        }

        if (transform.localPosition.z != startPos.z)
        {
            transform.localPosition = new Vector3(startPos.x, transform.localPosition.y, startPos.z);
        }
    }
}