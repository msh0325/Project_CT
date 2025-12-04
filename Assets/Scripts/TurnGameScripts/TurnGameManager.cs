using System.Collections.Generic;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("Data")]
    public CSVReader cSVReader;

    [Header("파티 구성")]
    public List<string> allyCharacterIDs;
    public List<string> enemyCharacterIDs;

    private List<BattleUnit> allies = new();
    private List<BattleUnit> enemies = new();
    private List<BattleUnit> turnOrder = new();
    
    private int currentTurnIndex = 0;
    private BattleState state = BattleState.Idle;

    void Start()
    {
        cSVReader.readData = ReadData.CharacterData;
        cSVReader.ReadCSV();
    }

    void Update()
    {
        
    }
}