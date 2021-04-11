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

        private List<Vector3> cornersList;
        private int cornersNearEachOtherCount;
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

        public void FindPath()
        {
            this.path = Astar.GetPath(startPoint, exitPoint, obstaclesArray, grid);
            this.cornersList = GetListOfCorners(this.path);
            this.cornersNearEachOtherCount = CalculateCornersNearEachOther(this.cornersList);
        }

        private int CalculateCornersNearEachOther(List<Vector3> cornersList)
        {
            int cornersNearsEachOther = 0;

            for (int i = 0; i < cornersList.Count - 1; i++)
            {
                if(Vector3.Distance(cornersList[i], cornersList[i + 1]) <= 1)
                {
                    cornersNearsEachOther++;
                }
            }
            return cornersNearsEachOther;
        }

        private List<Vector3> GetListOfCorners(List<Vector3> path)
        {
            List<Vector3> pathWithStart = new List<Vector3>(path);
            pathWithStart.Insert(0, startPoint);
            List<Vector3> cornersPositions = new List<Vector3>();

            if(pathWithStart.Count <= 0)
            {
                return cornersPositions;
            }

            for (int i = 1; i < pathWithStart.Count-2; i++)
            {
                float currentPathPositionX = pathWithStart[i].x;
                float previousPathPositionX = pathWithStart[i-1].x;
                float nextPathPositionX = pathWithStart[i+1].x;

                float currentPathPositionZ = pathWithStart[i].z;
                float previousPathPositionZ = pathWithStart[i-1].z;
                float nextPathPositionZ = pathWithStart[i+1].z;

                if (currentPathPositionX > previousPathPositionX
                || currentPathPositionX < previousPathPositionX)
                {
                    if(nextPathPositionZ > currentPathPositionZ 
                    || nextPathPositionZ < currentPathPositionZ)
                    {
                        cornersPositions.Add(pathWithStart[i]);
                    }
                }
                else if (currentPathPositionZ > previousPathPositionZ
                || currentPathPositionZ < previousPathPositionZ)
                {
                    if (nextPathPositionX > currentPathPositionX
                    || nextPathPositionX < currentPathPositionX)
                    {
                        cornersPositions.Add(pathWithStart[i]);
                    }
                }
            }
            return cornersPositions;
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

        public MapData ReturnMapData()
        {
            return new MapData
            {
                obstacleArray = this.obstaclesArray,
                knightPieceList = knightPiecesList,
                startPosition = startPoint,
                exitPosition = exitPoint,
                path = this.path,
                cornersList = this.cornersList
            };
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
                exitPosition = exitPoint,
                path = this.path,
                cornersList = this.cornersList,
                cornersNearEachOther = this.cornersNearEachOtherCount
            };
        }

        public bool IsObstacleAt(int i)
        {
            return obstaclesArray[i];
        }

        public void PlaceObstacle(int i, bool isObstacle)
        {
            obstaclesArray[i] = isObstacle;
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

        public void AddMutation(double mutationRate)
        {
            int numItems = (int) (obstaclesArray.Length * mutationRate);
            while(numItems > 0)
            {
                int randomIndex = Random.Range(0, obstaclesArray.Length);
                obstaclesArray[randomIndex] = !obstaclesArray[randomIndex];
                numItems--;
            }
        }

        public CandidateMap DeepClone()
        {
            return new CandidateMap(this);
        }

        public CandidateMap(CandidateMap candidateMap)
        {
            this.grid = candidateMap.grid;
            this.startPoint = candidateMap.startPoint;
            this.exitPoint = candidateMap.exitPoint;
            this.obstaclesArray = (bool[])candidateMap.obstaclesArray.Clone();
            this.cornersList = new List<Vector3>(candidateMap.cornersList);
            this.cornersNearEachOtherCount = candidateMap.cornersNearEachOtherCount;
            this.path = new List<Vector3>(candidateMap.path);
        }

    }
}
