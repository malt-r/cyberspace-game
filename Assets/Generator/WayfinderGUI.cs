using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR_WIN
namespace Assets
{
    [CustomEditor(typeof(Wayfinder))]
    class WayfinderGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Wayfinder helper = target as Wayfinder;

            if (GUILayout.Button("Init"))
            {
                helper.Init();
            }

            if (GUILayout.Button("Help"))
            {
                helper.FindWay();
            }
        }
    }
}
#endif
