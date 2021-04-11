using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ninja.ChessMaze
{
    public class MapBrain : MonoBehaviour
    {
        //Genetic algorithm parameters
        [SerializeField, Range(20, 100)]
        private int populationSize = 20;

        [SerializeField, Range(0, 100)]
        private int crossoverRate = 100;

        private double crossoverRatePercent;

        [SerializeField, Range(0, 100)]
        private int mutationRate = 0;

        private double mutationRatePercent;

        [SerializeField, Range(1, 100)]
        private int generationLimit = 10;

        //algorithm variables
        private List<CandidateMap> currentGeneration;
        private int totalFitnessThisGeneration, bestFitnessScoreAllTime = 0;
        private CandidateMap bestMap = null;
        private int bestMapGenerationNumber = 0, generatioNumber = 1;

        //fitness parameters
        [SerializeField]
        private int fitnessCornerMin = 6, fitnessCornerMax = 12;

        [SerializeField, Range(1, 3)]
        private int fitnessCornerWeight = 1, fitnessNearestCornerWeight = 1;

        [SerializeField, Range(1, 5)]
        private int fitnessPathWeight = 1;

        [SerializeField, Range(0.3f, 1f)]
        private float fitnessObstacleWeight = 1;

        //Map start parameters
        [SerializeField, Range(3, 20)]
        private int widthOfMap = 11, lenghtOfMap = 11;

        private Vector3 startPosition, exitPosition;

        private MapGrid grid;

        [SerializeField]
        private Direction startPositionEdge = Direction.Left, exitPositionEdge = Direction.Right;

        [SerializeField]
        private bool randomStartAndEnd = false;

        [SerializeField, Range(4, 9)]
        private int numberOfKnightPieces = 7;

        //Visualize grid
        public MapVisualizer mapVisualizer;

        DateTime startDate, endDate;

        private bool isAlgorithmRunning = false;

        public bool IsAlgorithmRunning { get => isAlgorithmRunning; }


        private void Start()
        {
            mutationRatePercent = mutationRate / 100D;
            crossoverRatePercent = crossoverRate / 100D;
            //RunAlgorithm();
        }

        public void RunAlgorithm()
        {
            UIController.instance.ResetScreen();
            ResetAlgorithmVariables();
            mapVisualizer.ClearMap();

            grid = new MapGrid(widthOfMap, lenghtOfMap);

            MapHelper.RandomlyChooseAndSetStartAndExit(grid, ref startPosition, ref exitPosition,
                                                        randomStartAndEnd, startPositionEdge, exitPositionEdge);

            isAlgorithmRunning = true;
            startDate = DateTime.Now;
            FindOptimalSolution(grid);
        }

        private void ResetAlgorithmVariables()
        {
            totalFitnessThisGeneration = 0;
            bestFitnessScoreAllTime = 0;
            bestMap = null;
            bestMapGenerationNumber = 0;
            generatioNumber = 0;
        }

        private void FindOptimalSolution(MapGrid grid)
        {
            currentGeneration = new List<CandidateMap>(populationSize);

            for (int i = 0; i < populationSize; i++)
            {
                var candidateMap = new CandidateMap(grid, numberOfKnightPieces);
                candidateMap.CreateMap(startPosition, exitPosition, true);

                currentGeneration.Add(candidateMap);
            }

            StartCoroutine(GeneticAlgorithm());
        }

        private IEnumerator GeneticAlgorithm()
        {
            totalFitnessThisGeneration = 0;
            int bestFitnessScoreThisGeneration = 0;
            CandidateMap bestMapThisGeneration = null;

            foreach (var candidate in currentGeneration)
            {
                candidate.FindPath();
                candidate.Repair();
                int fitness = CalculateFitness(candidate.ReturnMapData());

                totalFitnessThisGeneration += fitness;
                if(fitness > bestFitnessScoreThisGeneration)
                {
                    bestFitnessScoreThisGeneration = fitness;
                    bestMapThisGeneration = candidate;
                }
            }

            if(bestFitnessScoreThisGeneration > bestFitnessScoreAllTime)
            {
                bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
                bestMap = bestMapThisGeneration.DeepClone();
                bestMapGenerationNumber = generatioNumber;
            }

            generatioNumber++;
            yield return new WaitForEndOfFrame();
            UIController.instance.SetLoadingValue(generatioNumber / (float)generationLimit);

            Debug.Log("Best of generation " + generatioNumber + "| Score: " + bestMapGenerationNumber);

            if(generatioNumber < generationLimit)
            {
                List<CandidateMap> nextGeneration = new List<CandidateMap>();

                while(nextGeneration.Count < populationSize) 
                {
                    var parent1 = currentGeneration[RouletteWheelSelection()];
                    var parent2 = currentGeneration[RouletteWheelSelection()];

                    CandidateMap child1, child2;

                    CrossOverParents(parent1, parent2, out child1, out child2);

                    child1.AddMutation(mutationRatePercent);
                    child2.AddMutation(mutationRatePercent);

                    nextGeneration.Add(child1);
                    nextGeneration.Add(child2);
                }

                currentGeneration = nextGeneration;

                StartCoroutine(GeneticAlgorithm());
            }
            else
            {
                ShowResults();
            }


        }

        private void ShowResults()
        {
            isAlgorithmRunning = false;
            Debug.Log("Best solution at generation: " + bestMapGenerationNumber + " with score: " + bestFitnessScoreAllTime);

            var data = bestMap.ReturnMapData();
            mapVisualizer.ClearMap();
            mapVisualizer.VisualizeMap(bestMap.Grid, data, true);

            UIController.instance.HideLoadingScreen();

            Debug.Log("Path lenght: " + data.path.Count);
            Debug.Log("Corners count: " + data.cornersList.Count);

            endDate = DateTime.Now;
            long elapsedTicks = endDate.Ticks - startDate.Ticks;
            TimeSpan elapseSpan = new TimeSpan(elapsedTicks);

            Debug.Log("Time needed to run this genetic optimization: " + elapseSpan.TotalSeconds);
        }

        private void CrossOverParents(CandidateMap parent1, CandidateMap parent2, out CandidateMap child1, out CandidateMap child2)
        {
            child1 = parent1.DeepClone();
            child2 = parent2.DeepClone();

            if(Random.value < crossoverRatePercent)
            {
                int numBits = parent1.ObstaclesArray.Length;

                int crossOverIndex = Random.Range(0, numBits);

                for (int i = crossOverIndex; i < numBits; i++)
                {
                    child1.PlaceObstacle(i, parent2.IsObstacleAt(i));
                    child2.PlaceObstacle(i, parent1.IsObstacleAt(i));
                }
            }
        }

        private int RouletteWheelSelection()
        {
            int randomValue = Random.Range(0, totalFitnessThisGeneration);

            for (int i = 0; i < populationSize; i++)
            {
                randomValue -= CalculateFitness(currentGeneration[i].ReturnMapData());

                if(randomValue <= 0)
                {
                    return i;
                }
            }
            return populationSize - 1;
        }

        private int CalculateFitness(MapData mapData)
        {
            int numberOfObstacles = mapData.obstacleArray.Where(isObstacle => isObstacle).Count();
            int score = mapData.path.Count * fitnessPathWeight + (int)(numberOfObstacles * fitnessObstacleWeight);
            int cornersCount = mapData.cornersList.Count;

            if(cornersCount >= fitnessCornerMin && cornersCount <= fitnessCornerMax)
            {
                score += cornersCount * fitnessCornerWeight;
            }
            else if(cornersCount > fitnessCornerMax)
            {
                score -= fitnessCornerWeight * (cornersCount - fitnessCornerMax);
            }
            else if(cornersCount < fitnessCornerMin)
            {
                score -= fitnessCornerWeight * fitnessCornerMin;
            }
            score -= mapData.cornersNearEachOther * fitnessNearestCornerWeight;
            return score;
        }
    }

}
