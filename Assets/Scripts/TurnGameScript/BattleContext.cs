using UnityEngine;

public class BattleContext : MonoBehaviour
{
    public int currentWave;
    public int currentRound;

    public void NextWave() => currentWave++;
    public void NextRound() => currentRound++;
}
