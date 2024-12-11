using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BehaviorStats
{
    [Tooltip("Distance at which the enemy will start chasing the target from the idle state. Making this value larger than the object's FieldOfView distance has no effect.")]
    [SerializeField] private float _startChaseDist;
    [Tooltip("Additional distance beyond the starting chase distance at which the enemy will stop chasing and return to idle.")]
    [SerializeField] private float _chaseBuffer;
    [Tooltip("Distance at which the enemy will start aiming at the target from the chasing state. Should be no greater than the starting chase distance.")]
    [SerializeField] private float _startAimDist;
    [Tooltip("Additional distance beyone the starting aim distance at which the enemy will stop aiming and return to chasing.")]
    [SerializeField] private float _aimBuffer;

    private float _endChaseDist;
    private float _endAimDist;

    public float StartChaseDist => _startChaseDist;
    public float ChaseBuffer => _chaseBuffer;
    public float StartAimDist => _startAimDist;
    public float AimBuffer => _aimBuffer;

    public float EndChaseDist => _startChaseDist + _chaseBuffer;
    public float EndAimDist => _startAimDist + _aimBuffer;
}
