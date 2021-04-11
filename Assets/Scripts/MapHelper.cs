using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ninja.ChessMaze
{
    public static class MapHelper
    {
        public static void RandomlyChooseAndSetStartAndExit(MapGrid grid, ref Vector3 startPosition, ref Vector3 exitPosition,
                                                            bool randomPlacement, Direction startPositionEdge = Direction.Left,
                                                            Direction exitPositionEdge = Direction.Right)
        {
            if (randomPlacement)
            {
                if (startPositionEdge.Equals(exitPositionEdge))
                {
                    startPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, startPosition, startPositionEdge);
                    exitPosition = RandomlyChoosePositionOnTheEdgeOfTheGridSameDirection(grid, exitPosition, startPosition, exitPositionEdge);
                }
                startPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, startPosition);
                exitPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, exitPosition);
            }
            else
            {
                if (startPositionEdge.Equals(exitPositionEdge))
                {
                    startPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, startPosition, startPositionEdge);
                    exitPosition = RandomlyChoosePositionOnTheEdgeOfTheGridSameDirection(grid, exitPosition, startPosition, exitPositionEdge);
                }
                else
                {
                    startPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, startPosition, startPositionEdge);
                    exitPosition = RandomlyChoosePositionOnTheEdgeOfTheGrid(grid, exitPosition, exitPositionEdge);
                }
            }

            grid.SetCell(startPosition.x, startPosition.z, CellObjectType.Start, true);
            grid.SetCell(exitPosition.x, exitPosition.z, CellObjectType.Exit, true);
        }

        private static Vector3 RandomlyChoosePositionOnTheEdgeOfTheGridSameDirection(MapGrid grid, Vector3 givenPosition, Vector3 startPosition, Direction direction)
        {
            if (direction == Direction.None)
            {
                direction = (Direction)Random.Range(1, 5);
            }

            Vector3 position = Vector3.zero;

            switch (direction)
            {
                case Direction.Right:
                    do
                    {
                        position = new Vector3(grid.Width - 1, 0, Random.Range(0, grid.Length));
                    } while (Vector3.Distance(position, startPosition) <= 1);
                    break;
                case Direction.Left:
                    do
                    {
                        position = new Vector3(0, 0, Random.Range(0, grid.Length));
                    } while (Vector3.Distance(position, startPosition) <= 1);
                    break;
                case Direction.Up:
                    do
                    {
                        position = new Vector3(Random.Range(0, grid.Width), 0, grid.Length - 1);
                    } while (Vector3.Distance(position, startPosition) <= 1);
                    break;
                case Direction.Down:
                    do
                    {
                        position = new Vector3(Random.Range(0, grid.Width), 0, 0);
                    } while (Vector3.Distance(position, startPosition) <= 1);
                    break;
                default:
                    break;
            }
            Debug.Log("Distance: " + Vector3.Distance(position, startPosition) );
            return position;

        }

        private static Vector3 RandomlyChoosePositionOnTheEdgeOfTheGrid(MapGrid grid, Vector3 givenPosition, Direction direction = Direction.None)
        {
            if(direction == Direction.None)
            {
                direction = (Direction)Random.Range(1, 5);
            }

            Vector3 position = Vector3.zero;

            switch (direction)
            {
                case Direction.Right:
                    do
                    {
                        position = new Vector3(grid.Width - 1, 0, Random.Range(0, grid.Length));
                    } while (Vector3.Distance(position, givenPosition) <= 1);
                    break;
                case Direction.Left:
                    do
                    {
                        position = new Vector3(0, 0, Random.Range(0, grid.Length));
                    } while (Vector3.Distance(position, givenPosition) <= 1);
                    break;
                case Direction.Up:
                    do
                    {
                        position = new Vector3(Random.Range(0, grid.Width), 0, grid.Length - 1);
                    } while (Vector3.Distance(position, givenPosition) <= 1);
                    break;
                case Direction.Down:
                    do
                    {
                        position = new Vector3(Random.Range(0, grid.Width), 0, 0);
                    } while (Vector3.Distance(position, givenPosition) <= 1);
                    break;
                default:
                    break;
            }

            return position;
        }
    }

}
