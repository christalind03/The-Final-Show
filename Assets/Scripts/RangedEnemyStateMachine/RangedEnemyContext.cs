using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyContext
{
    public readonly float AttackDamage;
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

    public RangedEnemyContext(float attackDamage, float startChaseDist, float endChaseDist, float startAttackDist, float endAttackDist,
        Vector3 initialPosition, Quaternion initialRoatation, Transform transform, FieldOfView fieldOfView, NavMeshAgent navMeshAgent, Material material)
    {
        AttackDamage = attackDamage;
        StartChaseDist = startChaseDist;
        EndChaseDist = endChaseDist;
        StartAttackDist = startAttackDist;
        EndAttackDist = endAttackDist;

        InitialPosition = initialPosition;
        InitialRotation = initialRoatation;
        Transform = transform;

        FieldOfView = fieldOfView;
        NavMeshAgent = navMeshAgent;
        Material = material;
    }
}