using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Door : MonoBehaviour
{
    private bool _inTrigger;
    private bool _isOpen;
    
    [Header("Settings")]
    public float smooth = 4.0f;
    public float doorOpenAngle = 90.0f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("UI Feedback")]
    public MonoBehaviour interactionText; // Supports legacy Text or TMP_Text

    private Vector3 _defaultRot;
    private Vector3 _openRot;

    private void Start()
    {
        _defaultRot = transform.eulerAngles;
        // Adjust based on typical rotation pivots. 
        // Note: Using localEulerAngles is usually safer for child objects, but let's stick to world if that's how it was.
        _openRot = new Vector3(_defaultRot.x, _defaultRot.y + doorOpenAngle, _defaultRot.z);
        SetText("");
    }

    private void Update()
    {
        // Smooth rotation
        Vector3 targetRot = _isOpen ? _openRot : _defaultRot;
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetRot, Time.deltaTime * smooth);

        // Interaction
        if (_inTrigger)
        {
            if (Input.GetKeyDown(interactKey))
            {
                _isOpen = !_isOpen;
            }

            SetText(_isOpen ? "Close " + interactKey : "Open " + interactKey);
        }
    }

    private void SetText(string msg)
    {
        if (interactionText == null) return;

        if (interactionText is Text legacy) legacy.text = msg;
        else if (interactionText is TMP_Text tmp) tmp.text = msg;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _inTrigger = false;
            SetText("");
        }
    }
}
