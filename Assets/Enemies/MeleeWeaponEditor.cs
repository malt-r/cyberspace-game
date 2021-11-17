using Assets.Weapons;
using UnityEditor;
using UnityEngine;
namespace Assets.Enemies
{

    [CustomEditor(typeof(MeleeWeapon))]
    class WeaponGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var weapon = target as BaseWeapon;

            GUILayout.Space(20);
            GUILayout.Label("Weapon controls");
            GUILayout.Label("First press this:");
            if (GUILayout.Button("Use"))
            {
                if (weapon != null) weapon.Use();
            }


        }

    }
}