using Framework.Runtime.Core.ContextBuilder;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Core
{
    [CustomEditor(typeof(UnityDebugServiceObserver))]
    public class UnityDebugServiceObserverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var observer = (UnityDebugServiceObserver)target;
            foreach (var service in observer.Services)
            {
                EditorGUILayout.LabelField(service.GetType().Name, $"{service.State}");
            }
        }
    }
}
