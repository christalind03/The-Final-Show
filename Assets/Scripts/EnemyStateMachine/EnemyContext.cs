using UnityEngine;
using UnityEngine.AI;

public class EnemyContext
{
    public readonly AttackStats AttackStats;
    public readonly float StartChaseDist;
    public readonly float EndChaseDist;
    public readonly float StartAttackDist;
    public readonly float EndAttackDist;

    public readonly Vector3 InitialPosition;
    public readonly Quaternion InitialRotation;
    public readonly Transform Transform;

    public readonly FieldOfView FieldOfView;
    public readonly NavMeshAgent NavMeshAgent;

    public Transform TargetTransform;
    public Material Material;

    public EnemyContext(AttackStats attackStats, Vector3 initialPosition, Quaternion initialRoatation, Transform transform, 
        FieldOfView fieldOfView, NavMeshAgent navMeshAgent, Material material)
    {
        AttackStats = attackStats;
        InitialPosition = initialPosition;
        InitialRotation = initialRoatation;
        Transform = transform;

        FieldOfView = fieldOfView;
        NavMeshAgent = navMeshAgent;
        Material = material;
    }
}
