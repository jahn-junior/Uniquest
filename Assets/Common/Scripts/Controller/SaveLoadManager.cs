using System.Collections.Generic;
using Common.Scripts.Maze;
using Singleton;
using UnityEngine;

namespace Common.Scripts.Controller
{
    public class SaveLoadManager : MonoBehaviour
    {
        // [SerializeField] 
        // private GameObject myNoSaveGameNotification;

        private DoorController myDoorController;
        private global::Maze myMaze;
        private PlayerController myPlayerController;
        private Door myDoor;

        private void Start()
        {
            myMaze = GameObject.Find("Maze").GetComponent<global::Maze>();
            myPlayerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
            // CheckForSavedGame();
        }
        
        public void SaveGame()
        {
            if (myMaze == null)
            {
                Debug.LogError("Maze object is not initialized.");
                return;
            }

            // Save Player specific data
            PlayerPrefs.SetFloat("PlayerSpeed", myPlayerController.MySpeed);
            PlayerPrefs.SetInt("PlayerItemCount", myPlayerController.MyItemCount);
            PlayerPrefs.SetString("PlayerPosition", JsonUtility.ToJson(myPlayerController.MyCharacterTransform.position));

            // Save door states in maze
            foreach (var doorController in myMaze.GetComponentsInChildren<DoorController>()) // Assuming the doors are children of the maze
            {
                SaveDoorState(doorController);
            }
            
            SaveMinimap();

            PlayerPrefs.Save();
        }

        public void LoadGame()
        {
            // Load Player specific data
            myPlayerController.myCharacterTransform.position = JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString("PlayerPosition"));
            myPlayerController.MySpeed = PlayerPrefs.GetFloat("PlayerSpeed",  myPlayerController.MySpeed);
            myPlayerController.MyItemCount =
                PlayerPrefs.GetInt("PlayerItemCount", myPlayerController.MyItemCount);

            // Load door states in maze
            foreach (var doorController in myMaze.GetComponentsInChildren<DoorController>())
            {
                LoadDoorState(doorController);
            }
            
            LoadMinimap();
            
            // Load question states in maze
            QuestionFactory.MyInstance.InitializeQuestionsFromSave();
        }

        
        public void NewGame() 
        { 
            if (IsSavedGameAvailable()) 
            { 
                // Delete saved data if it exists
                DeleteSavedGame(); 
            } 
            ResetGameState(); 
        } 
        
        private void DeleteSavedGame() 
        {
            if (PlayerPrefs.HasKey("PlayerSpeed"))
            {
                PlayerPrefs.DeleteKey("PlayerSpeed");
            }

            if (PlayerPrefs.HasKey("PlayerItemCount"))
            {
                PlayerPrefs.DeleteKey("PlayerItemCount");
            }

            if (PlayerPrefs.HasKey("PlayerPosition"))
            {
                PlayerPrefs.DeleteKey("PlayerPosition");
            }

            ResetDoorState(); 
            ResetMinimap();
            
            PlayerPrefs.Save(); 
        }

        private bool IsSavedGameAvailable()
        {
            // Check if any one of the saved game keys exist, return true. Otherwise return false.
            // This is just a sample, add more keys if needed to confirm a saved game
            return PlayerPrefs.HasKey("PlayerSpeed") || PlayerPrefs.HasKey("PlayerItemCount") ||
                   PlayerPrefs.HasKey("PlayerPosition");
        }
        
        // private void CheckForSavedGame()
        // {
        //     if (!PlayerPrefs.HasKey("PlayerPosition")) 
        //     {
        //         DisplayNoSaveGameNotification();
        //     }
        // }
        //
        // private void DisplayNoSaveGameNotification()
        // {
        //     myNoSaveGameNotification.SetActive(true);
        // }

        private void ResetGameState() 
        {
            // Reset player attributes to default values
            myPlayerController.MySpeed = 50f; 
            myPlayerController.MyRotationSpeed = 5f; 
            myPlayerController.MyCanMove = true; 
            myPlayerController.MyItemCount = 0; 
            // Reset player's position, rotation, and scale
            Vector3 defaultPosition = new Vector3(505, 1, 619); // Default character position
            myPlayerController.myCharacterTransform.position = defaultPosition; 
            myPlayerController.MyCharacterTransform.rotation = Quaternion.identity; // Resets to no rotation
            Vector3 defaultScale = new Vector3(1, 1, 1); 
            myPlayerController.MyCharacterTransform.localScale = defaultScale;

            // Reset animator state
            myPlayerController.MyAnimator.SetBool("isWalking", false);

            // Reset the isAnswered column in the SQLite database
            DataService.ResetQuestionStateInDatabase(); 
        }

        private void SaveDoorState(DoorController doorController) 
        { 
            myDoor = doorController.GetComponent<Door>();

            string doorID = doorController.myDoorID;

            PlayerPrefs.SetInt(doorID + "_LockState", myDoor.MyLockState ? 1 : 0);
            PlayerPrefs.SetInt(doorID + "_HasAttempted", myDoor.MyHasAttempted ? 1 : 0);

            Transform doorTransform = doorController.transform;

            // Save position
            PlayerPrefs.SetFloat(doorID + "_PosX", doorTransform.position.x);
            PlayerPrefs.SetFloat(doorID + "_PosY", doorTransform.position.y);
            PlayerPrefs.SetFloat(doorID + "_PosZ", doorTransform.position.z);

            // Save rotation
            PlayerPrefs.SetFloat(doorID + "_RotX", doorTransform.eulerAngles.x);
            PlayerPrefs.SetFloat(doorID + "_RotY", doorTransform.eulerAngles.y);
            PlayerPrefs.SetFloat(doorID + "_RotZ", doorTransform.eulerAngles.z);

            // Save scale
            PlayerPrefs.SetFloat(doorID + "_ScaleX", doorTransform.localScale.x);
            PlayerPrefs.SetFloat(doorID + "_ScaleY", doorTransform.localScale.y);
            PlayerPrefs.SetFloat(doorID + "_ScaleZ", doorTransform.localScale.z);
        }

        private void LoadDoorState(DoorController doorController)
        {
            myDoor = doorController.GetComponent<Door>();

            string doorID = doorController.myDoorID;

            // Check if the PlayerPrefs has the necessary key to determine if the door's state was saved previously
            if (PlayerPrefs.HasKey(doorID + "_LockState"))
            {
                // Load lock state and attempted state
                myDoor.MyLockState = PlayerPrefs.GetInt(doorID + "_LockState") == 1;
                myDoor.MyHasAttempted = PlayerPrefs.GetInt(doorID + "_HasAttempted") == 1;

                // Load position
                Vector3 position;
                position.x = PlayerPrefs.GetFloat(doorID + "_PosX");
                position.y = PlayerPrefs.GetFloat(doorID + "_PosY");
                position.z = PlayerPrefs.GetFloat(doorID + "_PosZ");
                doorController.transform.position = position;

                // Load rotation
                Vector3 eulerAngles;
                eulerAngles.x = PlayerPrefs.GetFloat(doorID + "_RotX");
                eulerAngles.y = PlayerPrefs.GetFloat(doorID + "_RotY");
                eulerAngles.z = PlayerPrefs.GetFloat(doorID + "_RotZ");
                doorController.transform.rotation = Quaternion.Euler(eulerAngles);

                // Load scale
                Vector3 scale;
                scale.x = PlayerPrefs.GetFloat(doorID + "_ScaleX");
                scale.y = PlayerPrefs.GetFloat(doorID + "_ScaleY");
                scale.z = PlayerPrefs.GetFloat(doorID + "_ScaleZ");
                doorController.transform.localScale = scale;
            }
            else
            {
                Debug.Log("No saved state found for door with ID: " + doorID);
            }
        }
        
        private void ResetDoorState()
        {
            foreach (var doorController in myMaze.myAllDoors)
            {
                Door door = doorController.GetComponent<Door>();
        
                if(door != null)
                {
                    door.MyOpenState = false;
                    door.MyLockState = true;
                    door.MyHasAttempted = false;
                    door.MyProximityTrigger = false;
                    if (door.MyAnimation != null)
                    {
                        StopCoroutine(door.MyAnimation);
                    }
                    doorController.transform.rotation = Quaternion.Euler(door.MyStartingRotation);
                    door.MyNavPopup.SetActive(false);
                }
            }
        }

        private void SaveMinimap()
        {
            Room[] allRooms = FindObjectsOfType<Room>();
            List<string> visitedRooms = new List<string>();
            foreach (Room room in allRooms)
            {
                if (room.MyHasVisited)
                {
                    visitedRooms.Add($"{room.MyRow},{room.MyCol}");
                }
            }

            PlayerPrefs.SetString("VisitedRooms", string.Join(";", visitedRooms));
            SaveMinimapDoors();
            PlayerPrefs.Save();
        }

        private void LoadMinimap()
        {
            if (PlayerPrefs.HasKey("VisitedRooms"))
            {
                string savedData = PlayerPrefs.GetString("VisitedRooms");
                List<Vector2Int> visitedRooms = new List<Vector2Int>();

                foreach (string roomCoord in savedData.Split(';'))
                {
                    string[] coords = roomCoord.Split(',');
                    visitedRooms.Add(new Vector2Int(int.Parse(coords[0]), int.Parse(coords[1])));
                }

                Room[] allRooms = FindObjectsOfType<Room>();
                foreach (Room room in allRooms)
                {
                    room.MyHasVisited = visitedRooms.Contains(new Vector2Int(room.MyRow, room.MyCol));
                }
            }
            LoadMinimapDoors();
        }

        private void SaveMinimapDoors()
        {
            Door[] allDoors = FindObjectsOfType<Door>();
            foreach (Door door in allDoors)
            {
                string doorKeyBase = $"Door_{door.transform.position}"; // Consider a unique identifier for doors if possible
                PlayerPrefs.SetInt($"{doorKeyBase}_HasAttempted", door.MyHasAttempted ? 1 : 0);
                PlayerPrefs.SetInt($"{doorKeyBase}_LockState", door.MyLockState ? 1 : 0);
            }
            PlayerPrefs.Save();
        }
        
        private void LoadMinimapDoors()
        {
            Door[] allDoors = FindObjectsOfType<Door>();
            foreach (Door door in allDoors)
            {
                string doorKeyBase = $"Door_{door.transform.position}"; // Same unique identifier as used in save
                if (PlayerPrefs.HasKey($"{doorKeyBase}_HasAttempted"))
                {
                    door.MyHasAttempted = PlayerPrefs.GetInt($"{doorKeyBase}_HasAttempted") == 1;
                    door.MyLockState = PlayerPrefs.GetInt($"{doorKeyBase}_LockState") == 1;
                }
            }
        }
        
        private void ResetMinimap()
        {
            // Reset the visited state of all rooms
            Room[] allRooms = FindObjectsOfType<Room>();
            foreach (Room room in allRooms)
            {
                room.MyHasVisited = false;
            }
    
            // Reset the current room of the maze (this is optional, depending on your game mechanics)
            // GameObject.Find("Maze").GetComponent<Maze>().MyCurrentRoom = null; // or set to a default room

            // Force update the colors of all minimap cells
            MinimapCell[] allMinimapCells = FindObjectsOfType<MinimapCell>();
            foreach (MinimapCell cell in allMinimapCells)
            {
                cell.ColorChange();
            }
            
            ResetMinimapDoors();
            PlayerPrefs.Save(); 
        }

        private void ResetMinimapDoors()
        {
            // Delete saved doors' data
            Door[] allDoors = FindObjectsOfType<Door>();
            foreach (Door door in allDoors)
            {
                string doorKeyBase = $"Door_{door.transform.position}";
                if (PlayerPrefs.HasKey($"{doorKeyBase}_HasAttempted"))
                {
                    PlayerPrefs.DeleteKey($"{doorKeyBase}_HasAttempted");
                }

                if (PlayerPrefs.HasKey($"{doorKeyBase}_LockState"))
                {
                    PlayerPrefs.DeleteKey($"{doorKeyBase}_LockState");
                }
            }

            PlayerPrefs.Save(); // Ensure changes are saved
        }


    }
}