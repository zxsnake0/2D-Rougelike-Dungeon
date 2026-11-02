using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Room Settings")]
    public GameObject[] roomPrefabs;

    [Header("Player")]
    public Transform player;
    private PlayerMovement playerMovement;

    private GameObject currentRoom;
    private RoomController currentRoomController;

    private bool canTransition;
    private int previousRoomIndex = -1;

    void Start()
{
    playerMovement = player.GetComponent<PlayerMovement>();

    if (playerMovement == null)
    {
        Debug.LogError("PlayerMovement component not found!");
        return;
    }

    SpawnNextRoom();
}

    public void SpawnNextRoom()
    {
        canTransition = false;

        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

        if (roomPrefabs.Length == 0)
        {
            Debug.LogError("No room prefabs assigned!");
            return;
        }

        int randomIndex;

        if (roomPrefabs.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, roomPrefabs.Length);
            }
            while (randomIndex == previousRoomIndex);
        }
        else
        {
            randomIndex = 0;
        }

        previousRoomIndex = randomIndex;

        currentRoom = Instantiate(
            roomPrefabs[randomIndex],
            Vector3.zero,
            Quaternion.identity
        );

        currentRoomController =
            currentRoom.GetComponent<RoomController>();

        if (currentRoomController == null)
        {
            Debug.LogError("Room prefab has no RoomController!");
            return;
        }

        if (currentRoomController.playerSpawn == null)
        {
            Debug.LogError("Room has no player spawn point!");
            return;
        }

        playerMovement.TeleportTo(
            currentRoomController.playerSpawn.position
        );

        currentRoomController.Initialize(this);
    }

    public void RoomCompleted()
    {
        canTransition = true;

        Debug.Log("Room completed! Exit unlocked.");
    }

    public void TransitionToNextRoom()
    {
        if (!canTransition)
            return;

        SpawnNextRoom();
    }
}