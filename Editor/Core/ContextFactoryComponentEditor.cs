using Framework.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Core
{
    [CustomEditor(typeof(ContextFactoryComponent), true)]
    public class ContextFactoryComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();
            if (GUILayout.Button("Rebuild"))
            {
                var component = (ContextFactoryComponent)target;
                component.Rebuild();
            }
        }
    }
}