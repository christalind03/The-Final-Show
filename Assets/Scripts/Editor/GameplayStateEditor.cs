using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GameplayState), true)]
public class GameplayStateEditor : Editor
{
    // TODO: Documentation
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Locate all serialized properties
        SerializedProperty targetScene = serializedObject.FindProperty("TargetScene");

        SerializedProperty isTimed = serializedObject.FindProperty("IsTimed");
        SerializedProperty countdownMessage = serializedObject.FindProperty("CountdownMessage");
        SerializedProperty transitionState = serializedObject.FindProperty("TransitionState");

        // Draw the `Target Scene` property field
        EditorGUILayout.PropertyField(targetScene);

        // Draw the countdown-related fields, which are conditionally displayed and disabled when `Is Timed` is false
        EditorGUILayout.PropertyField(isTimed);

        if (isTimed.boolValue)
        {
            EditorGUILayout.PropertyField(countdownMessage);
            EditorGUILayout.PropertyField(transitionState);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(countdownMessage);
            EditorGUILayout.PropertyField(transitionState);
            EditorGUI.EndDisabledGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
