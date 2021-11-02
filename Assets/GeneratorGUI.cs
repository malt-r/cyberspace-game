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

            GUILayout.Space(20);
            GUILayout.Label("Generator controls");
            GUILayout.Label("First press this (will cleanup, create grid and read rooms):");
            if (GUILayout.Button("Setup"))
            {
                gen.Setup();
            }

            //if (GUILayout.Button("Place Room"))
            //{
            //    gen.PlaceRoomWithSelectedIdx();
            //}

            GUILayout.Label("Then press this:");
            if (GUILayout.Button("Place all Rooms"))
            {
                gen.PlaceRooms();
            }

            if (GUILayout.Button("Triangulate"))
            {
                gen.Triangulate();
            }


            GUILayout.Label("When finished press this:");
            if (GUILayout.Button("Cleanup"))
            {
                gen.Cleanup();
            }
        }
    }
}
