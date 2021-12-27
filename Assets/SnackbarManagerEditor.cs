using Assets.Weapons;
using UnityEditor;
using UnityEngine;
namespace Assets.Enemies
{
#if UNITY_EDITOR_WIN
    [CustomEditor(typeof(SnackbarManager))]
    class SnackbarManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var snackbar = target as SnackbarManager;

            GUILayout.Space(20);
            GUILayout.Label("Snackbar Controls");
            if (GUILayout.Button("Trigger Snackbar"))
            {
                if (snackbar != null) snackbar.DisplayMessage("Test");
            }
            
            if (GUILayout.Button("Hide Snackbar"))
            {
                if (snackbar != null) snackbar.HideMessage();
                SceneView.RepaintAll();
            }
        }
    }
#endif
}