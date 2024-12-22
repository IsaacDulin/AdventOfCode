using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TradeInfo = System.Collections.Generic.Dictionary<(int,int,int,int), long>;

public class Day22 : MonoBehaviour
{
    public TextAsset Input;

    [ContextMenu("Run Pt 1")]
    public void Run()
    {
        var startingSecrets = ParseStartingSecrets();
        long sum =0;
        foreach(var secret in startingSecrets)
        {
            var finalSecret = GetFinalNumber(secret, 2000);
            sum += finalSecret;
        }
        Debug.Log("Secret sum: " + sum);
    }

    [ContextMenu("Run Pt 2")]
    public void RunPt2()
    {
        var startingSecrets = ParseStartingSecrets();
        TradeInfo allTraderInfo = FindAggregateTradeInfo(startingSecrets);
        long max = allTraderInfo.Max(kvp => kvp.Value);
        Debug.Log("Max bananas: " + max);
    }

    public List<long> ParseStartingSecrets()
    {
        List<long> startingSecrets = new();
        var lines = Input.text.Split('\n');
        foreach(var line in lines)
        {
            startingSecrets.Add(long.Parse(line));
        }
        return startingSecrets;
    }

    public long GetFinalNumber(long startingSecret, int reps)
    {
        PsuedorandomGenerator generator = new();
        long secret= startingSecret;
        for (int i =0; i< reps; i++){
             secret= generator.GetNext(secret);
        }
        return secret;
    }

    public TradeInfo FindAggregateTradeInfo(List<long> startingSecrets)
    {
        TradeInfo tradeInfo = new();
        foreach(var secret in startingSecrets)
        {
            TradeInfo singleTradeInfo = FindTradeInfo(secret);
            foreach(var kvp in singleTradeInfo) {
                if (tradeInfo.ContainsKey(kvp.Key)){
                    tradeInfo[kvp.Key] += kvp.Value;
                }
                else{
                    tradeInfo[kvp.Key] = kvp.Value;
                }
            }
        }
        return tradeInfo;
    }


    public TradeInfo FindTradeInfo(long secret) 
    {
        PsuedorandomGenerator generator = new();
        int priorPrice = 0;
        int[] lastFourDeltas = new int[4]; // offset by one
        TradeInfo tradeInfo = new();
        int rollingIdx = 0;
        for (int i=0 ;i < 2000; i++) 
        {
            secret = generator.GetNext(secret);
            int price = GetPrice(secret);

            if (i > 0) // store the delta 
            {
                lastFourDeltas[rollingIdx] = price - priorPrice;
            }

            if (i > 4) // store the price associated with this delta sequence
            {
                var tuple = (lastFourDeltas[rollingIdx], lastFourDeltas[(rollingIdx + 3) % 4],lastFourDeltas[(rollingIdx + 2) % 4],lastFourDeltas[(rollingIdx + 1) % 4]);
                if (!tradeInfo.ContainsKey(tuple))
                {
                    tradeInfo[tuple] = price;
                }
            }

            // Update the prior price and the idx;
            priorPrice = price;
            rollingIdx = (rollingIdx + 1) % 4;
        }
        return tradeInfo;
    }

    public int GetPrice(long secret) 
    {
        return (int)(secret % 10); // 1s digit
    }

    public class PsuedorandomGenerator
    {
        public long GetNext(long secret)
        {
            // Step 1
            long intermediate = secret * 64;
            secret = Mix(secret, intermediate);
            secret = Prune(secret);

            // Step 2
            intermediate = secret / 32;
            secret = Mix(secret, intermediate);
            secret = Prune(secret);

            // Step 3
            intermediate = secret * 2048;
            secret = Mix(secret, intermediate);
            secret = Prune(secret);

            return secret;
        }

        private long Prune(long secret)
        {
            return secret % 16777216;
        }

        private long Mix(long secret, long operand)
        {
            return secret ^ operand;
        }
    }
}
