using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets
{
    [CustomEditor(typeof(DungeonGenerator))]
    class GeneratorGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DungeonGenerator gen = target as DungeonGenerator;

            if (GUILayout.Button("Create Grid"))
            {
                gen.CreateGrid();
            }

            if (GUILayout.Button("Read Rooms"))
            {
                gen.ReadRooms();
            }

            if (GUILayout.Button("Place Room"))
            {
                gen.PlaceFirstRoom();
            }

            if (GUILayout.Button("Cleanup"))
            {
                gen.Cleanup();
            }
        }
    }
}
