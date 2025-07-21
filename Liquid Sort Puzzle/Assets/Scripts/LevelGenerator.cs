using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    const int containerSizeOfBottle = 4; // Shader color size

    [Header("Settings")]
    [SerializeField] int numberOfBottles = 4;
    [SerializeField] int numberOfEmptyBottles = 2;

    [Header("Assign")]
    [SerializeField] LiquidData[] allLiquids;
    [SerializeField] Transform[] positionArray;

    [Header("Object Pooling")]
    [SerializeField] Transform bottleParent;
    [SerializeField] Bottle bottlePrefab;
    PoolElement<Bottle> bottlePool;


    readonly List<Bottle> bottles = new();

    void Start()
    {
        bottlePool = new(bottlePrefab, bottleParent, 8);

        LiquidTransferManager.OnLiquidTransferCompleted += OnLiquidTransferCompleted;
    }
    void OnDestroy()
    {
        LiquidTransferManager.OnLiquidTransferCompleted -= OnLiquidTransferCompleted;
    }

    public void GenerateBottles()
    {
        if (bottles.Count > 0) 
            ClearOldBottles();

        List<LiquidContainer> availableLiquids = SelectLiquids(numberOfBottles);

        // Spawn bottles and distribute liquids
        for (int i = 0; i < numberOfBottles; i++)
        {
            List<LiquidContainer> distributedContainers;

            // Add all availableLiquids if it's last bottle
            if (i == numberOfBottles - 1)
            {
                ShuffleUtility.Shuffle(availableLiquids);
                distributedContainers = new(availableLiquids);

                // Add missing containers as empty
                if (distributedContainers.Count < containerSizeOfBottle)
                {
                    for (int j = distributedContainers.Count; j < containerSizeOfBottle; j++)
                        distributedContainers.Add(new LiquidContainer(null, 0));
                }
            }
            // Distribute availableLiquids
            else distributedContainers = DistributeLiquids(availableLiquids);

            SpawnBottle(i, distributedContainers);
        }

        // Spawn empty bottles
        for (int i = 0; i < numberOfEmptyBottles; i++)
        {
            List<LiquidContainer> emptyContainers = new();

            for (int j = 0; j < containerSizeOfBottle; j++)
                emptyContainers.Add(new LiquidContainer(null, 0));

            int positionIndex = i + numberOfBottles;
            SpawnBottle(positionIndex, emptyContainers);
        }
    }

    List<LiquidContainer> DistributeLiquids(List<LiquidContainer> availableLiquids)
    {
        List<LiquidContainer> availablePool = new(availableLiquids);
        List<LiquidContainer> distributedContainers = new();
        float emptySpace = 1f;

        // Randomly distribute availableLiquids to new containers
        while (emptySpace > .01f && distributedContainers.Count < containerSizeOfBottle)
        {
            // Reset pool if it's empty
            if (availablePool.Count == 0)
                availablePool = new(availableLiquids);

            // Select random from pool
            LiquidContainer source = availablePool[Random.Range(0, availablePool.Count)];
            availablePool.Remove(source);

            // Calculate fill amount
            float randomFill = Random.Range(2, 6) / 10f; //From 0.2 to 0.5
            randomFill = Mathf.Round(randomFill * 10f) / 10f;
            randomFill = Mathf.Min(randomFill, source.amount, emptySpace);

            source.amount -= randomFill;
            emptySpace -= randomFill;

            LiquidContainer container = new(source.data, randomFill);
            distributedContainers.Add(container);

            // Remove source container from availableLiquids if it's empty
            if (source.amount <= .01)
                availableLiquids.Remove(source);
        }

        // Add missing containers as empty
        if (distributedContainers.Count < containerSizeOfBottle)
        {
            for (int i = distributedContainers.Count; i < containerSizeOfBottle; i++)
                distributedContainers.Add(new LiquidContainer(null, 0));
        }

        return distributedContainers;
    }

    void SpawnBottle(int positionIndex, List<LiquidContainer> containers)
    {
        Bottle bottle = bottlePool.Get();
        bottle.transform.position = positionArray[positionIndex].position;
        bottle.gameObject.SetActive(true);

        bottle.AddStartingLiquids(containers);
        bottles.Add(bottle);
    }

    List<LiquidContainer> SelectLiquids(int quantity)
    {
        List<LiquidContainer> result = new();
        List<LiquidData> liquidPool = new(allLiquids);

        for (int i = 0; i < quantity; i++)
        {
            LiquidData liquid;

            if (i < liquidPool.Count) liquid = liquidPool[i];
            else liquid = liquidPool[Random.Range(0, liquidPool.Count)];

            result.Add(new LiquidContainer(liquid));
        }

        return result;
    }


    void ClearOldBottles()
    {
        foreach (Bottle bottle in bottles)
        {
            if (bottle != null)
            {
                bottle.ClearBottle();
                bottlePool.Return(bottle);
            }
        }

        bottles.Clear();
    }

    #region Level Success (Temporary Location)
    void OnLiquidTransferCompleted()
    {
        ChecklevelCompleted();
    }

    void ChecklevelCompleted()
    {
        foreach (Bottle bottle in bottles)
        {
            if (!bottle.IsCompleted()) return;
        }

        Debug.Log("Level Won!");
    }
    #endregion
}
