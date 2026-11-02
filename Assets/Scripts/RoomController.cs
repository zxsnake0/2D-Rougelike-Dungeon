using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Room Positions")]
    public Transform playerSpawn;

    private int enemiesRemaining;

    private RoomManager roomManager;

    public void Initialize(RoomManager manager)
    {
        roomManager = manager;

        foreach (RoomExit exit in GetComponentsInChildren<RoomExit>(true))
        {
            exit.roomManager = manager;
        }

        EnemyDeathTracker[] enemies =
            GetComponentsInChildren<EnemyDeathTracker>(true);

        enemiesRemaining = enemies.Length;

        foreach (EnemyDeathTracker enemy in enemies)
        {
            enemy.Initialize(this);
        }

        Debug.Log("Room initialized. Enemies: " +
            enemiesRemaining);

        if (enemiesRemaining == 0)
        {
            RoomCleared();
        }
    }

    public void EnemyDefeated()
    {
        enemiesRemaining--;

        Debug.Log("Enemies remaining: " +
            enemiesRemaining);

        if (enemiesRemaining <= 0)
        {
            RoomCleared();
        }
    }

    private void RoomCleared()
    {
        Debug.Log("Room cleared!");

        if (roomManager != null)
        {
            roomManager.RoomCompleted();
        }
    }
}