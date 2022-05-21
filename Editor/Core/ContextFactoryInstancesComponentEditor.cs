using Framework.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Core
{
    [CustomEditor(typeof(ContextFactoryInstancesComponent), true)]
    public class ContextFactoryInstancesComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Separator();
            if (GUILayout.Button("Rebuild"))
            {
                var component = (ContextFactoryInstancesComponent)target;
                component.Rebuild();
            }
        }
    }
}