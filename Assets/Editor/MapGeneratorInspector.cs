using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ninja.ChessMaze
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorInspector : Editor
    {
        MapGenerator mapGenerator;

        private void OnEnable()
        {
            mapGenerator = (MapGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                if(GUILayout.Button("Generate new map"))
                {
                    mapGenerator.GenerateNewMap();
                }
            }
        }
    }

}
