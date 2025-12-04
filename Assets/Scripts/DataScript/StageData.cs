using UnityEngine;

public class StageData
{
    public string stageID {get;set;}
    public string stageName {get;set;}
}

public class WaveData
{
    public string stageID {get;set;}
    public int waveIndex {get;set;}
    public string[] enemyID {get;set;}
    public int[] enemyCount {get;set;}
}