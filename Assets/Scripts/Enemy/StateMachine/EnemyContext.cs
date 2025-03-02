using UnityEngine;
using UnityEngine.AI;

public class EnemyContext
{
    public readonly AudioManager AudioManager;
    public readonly AttackStats AttackStats;
    public readonly BehaviorStats BehaviorStats;

    public readonly Vector3 InitialPosition;
    public readonly Quaternion InitialRotation;
    public readonly Transform Transform;

    public readonly FieldOfView FieldOfView;
    public readonly NavMeshAgent NavMeshAgent;
    public readonly Animator Animator;

    public Transform TargetTransform;
    public Material Material;

    public EnemyContext(AudioManager audioManager, AttackStats attackStats, BehaviorStats behaviorStats, Vector3 initialPosition, Quaternion initialRoatation, Transform transform, 
        FieldOfView fieldOfView, NavMeshAgent navMeshAgent, Animator animator, Material material)
    {
        AudioManager = audioManager;
        AttackStats = attackStats;
        BehaviorStats = behaviorStats;
        InitialPosition = initialPosition;
        InitialRotation = initialRoatation;
        Transform = transform;

        FieldOfView = fieldOfView;
        NavMeshAgent = navMeshAgent;
        Animator = animator;
        Material = material;
    }
}
