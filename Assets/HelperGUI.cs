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
    [CustomEditor(typeof(Helper))]
    class HelperGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            Helper helper = target as Helper;

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
