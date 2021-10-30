﻿using System;
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

            if (GUILayout.Button("Setup"))
            {
                gen.CreateGrid();
                gen.ReadRooms();
            }

            if (GUILayout.Button("Place Room"))
            {
                gen.PlaceRoomWithSelectedIdx();
            }
            

            if (GUILayout.Button("Place all Rooms"))
            {
                gen.PlaceRooms();
            }

            if (GUILayout.Button("Cleanup"))
            {
                gen.Cleanup();
            }
        }
    }
}
