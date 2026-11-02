using UnityEngine;

public class EnemyDeathTracker : MonoBehaviour
{
    private RoomController room;
    private bool hasReportedDeath;

    public void Initialize(RoomController roomController)
    {
        room = roomController;
    }

    public void ReportDefeated()
    {
        if (hasReportedDeath)
            return;

        hasReportedDeath = true;

        if (room != null)
        {
            room.EnemyDefeated();
        }
    }
}