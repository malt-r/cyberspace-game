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
            //if (GUILayout.Button("Place all Rooms (try once)"))
            //{
            //    gen.PlaceRooms();
            //}

            if (GUILayout.Button("sudo Place all Rooms"))
            {
                gen.SudoPlaceRooms();
            }

            if (GUILayout.Button("Create Context Graph"))
            {
                gen.FindPaths();
            }

            if (GUILayout.Button("Place Corridors..."))
            {
                gen.PlaceCorridors();
            }

            if (GUILayout.Button("Instantiate Player"))
            {
                gen.InstantiatePlayer();
            }

            if (GUILayout.Button("MINIMAP"))
            {
                gen.CreateMinimap();
            }

            GUILayout.Label("When finished press this:");
            if (GUILayout.Button("Cleanup"))
            {
                gen.Cleanup();
            }

            GUILayout.Label("The Whole Operation, tries configured max dungeon tries");
            if (GUILayout.Button("Generate Dungeon"))
            {
                gen.GenerateDungeon();
            }
        }
    }

}
#endif
