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
                if (snackbar != null) snackbar.DisplayMessage("Leftclick<sprite index=69>DpD<sprite index=77>DpD<sprite index=11>DpD<sprite index=16>DpD<sprite index=30>DpD<sprite index=33>Hl<sprite index=35><sprite index=40><sprite index=55><sprite index=57><sprite index=78><sprite index=82><sprite index=85><sprite index=86><sprite index=90>Hallo");
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