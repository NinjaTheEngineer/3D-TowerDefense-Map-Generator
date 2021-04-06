using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ninja.ChessMaze
{
    public class CandidateMap
    {
        private MapGrid grid;
        private int numberOfPieces = 0;

        private Vector3 startPoint, exitPoint;
        private List<KnightPiece> knightPiecesList;

        private bool[] obstaclesArray = null;
        private List<Vector3> path = new List<Vector3>();

        public MapGrid Grid { get => grid; }
        public bool[] ObstaclesArray { get => obstaclesArray; }

        public CandidateMap(MapGrid grid, int numberOfPieces)
        {
            this.numberOfPieces = numberOfPieces;
            //this.knightPiecesList = new List<KnightPiece>();
            this.grid = grid;
        }

        public void CreateMap(Vector3 _startPosition, Vector3 _exitPosition, bool autoRepair = false)
        {
            this.startPoint = _startPosition;
            this.exitPoint = _exitPosition;

            this.obstaclesArray = new bool[grid.Width * grid.Length];
            this.knightPiecesList = new List<KnightPiece>();

            RandomlyPlaceKnightPieces(this.numberOfPieces);

            PlaceObstacles();
            FindPath();

            if (autoRepair)
            {
                Repair();
            }
        }

        private void FindPath()
        {
            this.path = Astar.GetPath(startPoint, exitPoint, obstaclesArray, grid);

            foreach(var position in this.path)
            {
                Debug.Log(position);
            }
        }

        private bool CheckIfPositionCanBeObstacle(Vector3 position)
        {
            if (position == startPoint || position == exitPoint)
            {
                return false;
            }

            int index = grid.CalculateIndexFromCoordinates(position.x, position.z);

            return obstaclesArray[index] == false;
        }

        private void RandomlyPlaceKnightPieces(int numberOfPieces)
        {
            var count = numberOfPieces;
            var knightPlacementTryLimit = 100;

            while (count > 0 && knightPlacementTryLimit > 0)
            {
                var randomIndex = Random.Range(0, obstaclesArray.Length);

                if (obstaclesArray[randomIndex] == false)
                {
                    var coordinates = grid.CalculateIndexFromCoordinatesFromIndex(randomIndex);

                    if (coordinates == startPoint || coordinates == exitPoint)
                    {
                        continue;
                    }

                    obstaclesArray[randomIndex] = true;
                    knightPiecesList.Add(new KnightPiece(coordinates));

                    count--;
                }

                knightPlacementTryLimit--;
            }

        }

        private void PlaceObstaclesForThisKnight(KnightPiece knight)
        {
            foreach (var position in KnightPiece.listOfPossibleMoves)
            {
                var newPosition = knight.Position + position;

                if(grid.IsCellValid(newPosition.x, newPosition.z) && CheckIfPositionCanBeObstacle(newPosition))
                {
                    obstaclesArray[grid.CalculateIndexFromCoordinates(newPosition.x, newPosition.z)] = true;
                }
            }
        }

        private void PlaceObstacles()
        {
            foreach(var knight in knightPiecesList)
            {
                PlaceObstaclesForThisKnight(knight);
            }
        }

        public MapData GetMapData()
        {
            return new MapData
            {
                obstacleArray = this.obstaclesArray,
                knightPieceList = knightPiecesList,
                startPosition = startPoint,
                exitPosition = exitPoint
            };
        }

        public List<Vector3> Repair()
        {
            int numberOfObstacles = obstaclesArray.Where(obstacle => obstacle).Count();

            List<Vector3> listOfObstaclesToRemove = new List<Vector3>();

            if(path.Count <= 0)
            {
                do
                {
                    int obstacleIndexToRemove = Random.Range(0, numberOfObstacles);

                    for (int i = 0; i < obstaclesArray.Length; i++)
                    {
                        if (obstaclesArray[i])
                        {
                            if(obstacleIndexToRemove == 0)
                            {
                                obstaclesArray[i] = false;
                                listOfObstaclesToRemove.Add(grid.CalculateIndexFromCoordinatesFromIndex(i));
                                break;
                            }

                            obstacleIndexToRemove--;
                        }

                    }

                    FindPath();
                } while (this.path.Count <= 0);
            }

            foreach (var obstacle in listOfObstaclesToRemove)
            {
                if(path.Contains(obstacle) == false)
                {
                    int index = grid.CalculateIndexFromCoordinates(obstacle.x, obstacle.z);
                    obstaclesArray[index] = true;
                }
            }

            return listOfObstaclesToRemove;
        }
    }
}
