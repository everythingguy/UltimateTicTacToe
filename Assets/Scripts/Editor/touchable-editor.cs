// Touchable_Editor component, to prevent treating the component as a Text object.

using UnityEditor;

[CustomEditor(typeof(InvisButton))]
public class InvisButton_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        // Do nothing
    }
}