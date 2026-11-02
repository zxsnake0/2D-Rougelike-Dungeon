using UnityEngine;

public class RoomExit : MonoBehaviour
{
    public RoomManager roomManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {

Debug.Log("Exit Triggered");
        if (collision.CompareTag("Player"))
        {
            roomManager.TransitionToNextRoom();
        }
    }
}