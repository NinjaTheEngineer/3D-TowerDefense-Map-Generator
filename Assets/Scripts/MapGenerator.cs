using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ninja.ChessMaze
{
    public class MapGenerator : MonoBehaviour
    {
        public GridVisualizer gridVisualizer;

        public MapVisualizer mapVisualizer;

        private Vector3 startPosition, exitPosition;
        public Direction startEdge, exitEdge;
        public bool randomPlacement;

        [Range(1, 10)]
        public int numberOfPieces = 3;

        [Range(3, 20)]
        public int width, length = 11;

        private MapGrid grid;

        private void Start()
        {


            gridVisualizer.VisualizeGrid(width, length);
            GenerateNewMap();

            //Debug.Log(startPosition);
            //Debug.Log(exitPosition);
        }

        public void GenerateNewMap()
        {
            mapVisualizer.ClearMap();
         
            grid = new MapGrid(width, length);
            
            MapHelper.RandomlyChooseAndSetStartAndExit(grid, ref startPosition, ref exitPosition, randomPlacement, startEdge, exitEdge);
            
            CandidateMap map = new CandidateMap(grid, numberOfPieces);
            map.CreateMap(startPosition, exitPosition);
            mapVisualizer.VisualizeMap(grid, map.GetMapData(), false);
        }
    }
}
