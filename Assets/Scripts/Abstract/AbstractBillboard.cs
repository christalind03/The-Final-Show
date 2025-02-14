using UnityEngine;

/// <summary>
/// An abstract, base class that ensures the attached gameObject (typically a plane) always faces the local player's camera.
/// https://school.gdquest.com/glossary/billboard
/// </summary>
/// <remarks>
/// Since we only want this behavior to occur locally, we inherit from MonoBehaviour as opposed to NetworkBehaviour (which syncs it with the server).
/// Additionally, since players are spawned in *after* this object is initialized, we must search for the local player's camera with some arbitrary number of attempts.
/// </remarks>
public class AbstractBillboard : MonoBehaviour
{
    private Transform _playerCamera;

    /// <summary>
    /// If the local player's camera exists, orient the object to face the camera's direction.
    /// Otherwise, attempt to locate it.
    /// </summary>
    /// <remarks>
    /// To avoid jitter caused by continuous camera updates in the <see cref="PlayerController"/>'s Update() function, we use LateUpdate() instead.
    /// Further explanation can be found in the following video: https://www.youtube.com/watch?v=BLfNP4Sc_iA&t=1120s (16:55)
    /// </remarks>
    protected virtual void LateUpdate()
    {
        if (_playerCamera != null)
        {
            transform.LookAt(transform.position + _playerCamera.forward);
        }
        else
        {
            AssignPlayerCamera();
        }
    }

    /// <summary>
    /// Attempts to find the player's camera and assigns it to <see cref="_playerCamera"/>
    /// </summary>
    private void AssignPlayerCamera()
    {
        GameObject localPlayer = NetworkUtils.RetrieveLocalPlayer();

        if (localPlayer != null)
        {
            Camera maybeCamera = localPlayer.GetComponentInChildren<Camera>();

            if (maybeCamera != null)
            {
                _playerCamera = maybeCamera.transform;
            }
        }
    }
}
