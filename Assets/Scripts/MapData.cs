using System.Collections.Generic;
using UnityEngine;

namespace Ninja.ChessMaze
{
    public struct MapData
    {
        public bool[] obstacleArray;
        public List<KnightPiece> knightPieceList;
        public Vector3 startPosition;
        public Vector3 exitPosition;
        public List<Vector3> path;
        public List<Vector3> cornersList;
        public int cornersNearEachOther;
    }
}
