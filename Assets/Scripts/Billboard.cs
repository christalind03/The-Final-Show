using UnityEngine;

public class Billboard : MonoBehaviour
{
    private int _maxAttempts;
    private int _currentAttempts;
    private Transform _playerCamera;

    // TODO: Document
    private void Start()
    {
        _maxAttempts = 3;
        _currentAttempts = 0;
    }

    // TODO: Document
    private void LateUpdate()
    {
        if (_playerCamera != null)
        {
            transform.LookAt(transform.position + _playerCamera.forward);
        }
        else if (_maxAttempts > _currentAttempts)
        {
            AssignPlayerCamera();
        }
    }

    // TODO: Document
    private void AssignPlayerCamera()
    {
        GameObject localPlayer = UnityExtensions.RetrieveLocalPlayer();

        if (localPlayer != null)
        {
            _playerCamera = localPlayer.GetComponentInChildren<Camera>().transform;
        }
        else if (_maxAttempts <= _currentAttempts)
        {
            UnityExtensions.LogError($"Unable to locate local player after {_maxAttempts} attempts.");
        }

        _currentAttempts++;
    }
}
