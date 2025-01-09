using UnityEditor;

[CustomEditor(typeof(InteractableGameplayController))]
public class InteractableGameplayControllerEditor : Editor
{
    // TODO: Documentation
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Locate all serialized properties
        SerializedProperty targetState = serializedObject.FindProperty("_targetState");

        SerializedProperty enableCountdown = serializedObject.FindProperty("_enableCountdown");
        SerializedProperty countdownProperties = serializedObject.FindProperty("_countdownProperties");

        // Draw the `Target State` property field
        EditorGUILayout.PropertyField(targetState);

        // Draw the countdown-related fields, which are conditionally displayed and disabled when `Is Timed` is false
        EditorGUILayout.PropertyField(enableCountdown);

        if (enableCountdown.boolValue)
        {
            EditorGUILayout.PropertyField(countdownProperties);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(countdownProperties);
            EditorGUI.EndDisabledGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
