using System.Collections;
using static System.Exception;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Maze mazePrefab;
    public Player playerPrefab;
    public LightSphere lightSpherePrefab;
    public Exit exitPrefab;
    public Image fade;
    public float fadeTime;
    public int numSpheres;
    public MainCamera mainCamera;
    public float timeToExit;
    public float timeToExitEps;
    public float maxLightSphereDistance;
    public float distanceToBlockSpheres;
    public float distanceToCatchSphere;
    public int gameStatus;

    private Maze maze;
    private Player player;
    private LightSphere[] lightSpheres;
    private Exit exit;
    private float currentFadeTime = 0;

    void Start() 
    {
        Cursor.visible = false;
        gameStatus = -1;

        Random.InitState(System.DateTime.Now.Second);
        maze = Instantiate(mazePrefab) as Maze;
        maze.name = "Maze";
        maze.Generate();

        player = Instantiate(playerPrefab) as Player;
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        mainCamera.player = player;

        lightSpheres = new LightSphere[numSpheres];
        for (int i = 0; i < numSpheres; ++i) {
            LightSphere newSphere = Instantiate(lightSpherePrefab) as LightSphere;
            lightSpheres[i] = newSphere;
            newSphere.name = "LightSphere " + i;
            newSphere.player = player;
        }

        exit = Instantiate(exitPrefab) as Exit;
        exit.name = "Exit";
        bool exitIsGood = false;
        float maxPossibleDistance = player.speed * timeToExit;
        int iters = 1000;

        while (!exitIsGood && iters > 0) {
            iters -= 1;
            exit.transform.position = maze.GetClosestPassage(
                new Vector3(
                    Random.Range(-maxPossibleDistance, maxPossibleDistance), 
                    1f, 
                    Random.Range(-maxPossibleDistance, maxPossibleDistance)));
            float distance = maze.FindPath(player.transform.position, exit.transform.position).Count * maze.scale.x;
            if (distance >= maxPossibleDistance * (1 - timeToExitEps) && distance <= maxPossibleDistance * (1 + timeToExitEps)) {
                exitIsGood = true;
                Debug.Log("Time to exit approximately " + distance / player.speed + " seconds, iters left " + iters);
            }
        }

        if (iters == 0) {
            throw new System.Exception("Failed to find spot for exit!");
        }
    }
    
    void Update() 
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        maze.StartLoadChunks();
        foreach (LightSphere lightSphere in lightSpheres) {
            maze.LoadChunks(lightSphere.transform.position);
        }
        maze.UnLoadChunks();

        if ((player.transform.position - exit.transform.position).magnitude < 0.5f) {
            gameStatus = 1;
        }

        foreach (LightSphere lightSphere in lightSpheres) {
            if ((player.transform.position - lightSphere.transform.position).magnitude < distanceToCatchSphere 
                && !lightSphere.connectedToPlayer && lightSphere.inPosition) {
                lightSphere.connectedToPlayer = true;
                lightSphere.attach.Play();
            }
        }

        if (Input.GetKeyDown("space")) {
            ReleaseSphere();
        }

        if (gameStatus == -1) {
            if (currentFadeTime < fadeTime) {
                fade.color = new Color(0, 0, 0, 1 - currentFadeTime / fadeTime);
            } else {
                fade.color = new Color(0, 0, 0, 0);
                gameStatus = 0;
            }
        } else if (gameStatus == 1) {
            if (currentFadeTime < fadeTime) {
                fade.color = new Color(0, 0, 0, currentFadeTime / fadeTime);
            } else {
                fade.color = new Color(0, 0, 0, 1);
                SceneManager.LoadScene("WinScreen");
            }
        } else if (gameStatus == 2) {
            Debug.Log("" + currentFadeTime);
            if (currentFadeTime < fadeTime) {
                fade.color = new Color(0, 0, 0, currentFadeTime / fadeTime);
            } else {
                fade.color = new Color(0, 0, 0, 1);
                SceneManager.LoadScene("LoseScreen");
            }
        } else {
            currentFadeTime = 0.0f;
        }
        currentFadeTime += Time.deltaTime;
    }

    private void ReleaseSphere() {
        Vector3 targetPosition = exit.transform.position;

        int firstConnected = -1;
        int lastNotConnected = -1;
        for (int i = 0; i < numSpheres; ++i) {
            if (lightSpheres[i].connectedToPlayer && firstConnected == -1) {
                firstConnected = i;
            } 
            if (!lightSpheres[i].connectedToPlayer) {
                lastNotConnected = i;
            }
        }

        if (lastNotConnected != -1) {
            targetPosition = lightSpheres[lastNotConnected].floorPosition;
        }

        if (firstConnected == -1) {
            return;
        }

        if (firstConnected == numSpheres - 1) {
            gameStatus = 2;
        }
        
        targetPosition = maze.GetClosestPassage(player.transform.position + 
            Vector3.ClampMagnitude(targetPosition - player.transform.position, maxLightSphereDistance));

        List<Vector3> path = maze.FindPath(player.transform.position, targetPosition);

        float distance = path.Count * maze.scale.x;
        float distanceForSphere;
        if (distance < distanceToBlockSpheres) {
            return;
        } else if (distance >= distanceToBlockSpheres && distance < maxLightSphereDistance + distanceToBlockSpheres / 2) {
            distanceForSphere = distance - distanceToBlockSpheres / 2;
        } else {
            distanceForSphere = maxLightSphereDistance;
        }
        distanceForSphere *= (numSpheres - 0.7f - firstConnected) / (numSpheres - 0.7f);
        lightSpheres[firstConnected].connectedToPlayer = false;
        lightSpheres[firstConnected].release.Play();
        lightSpheres[firstConnected].floorPosition = path[(int)(distanceForSphere / maze.scale.x)];
    }
}
