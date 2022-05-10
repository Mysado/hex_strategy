using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private HexMapGenerator HexMapGenerator;
    [SerializeField] private UnitsSpawner UnitsSpawner;
    [SerializeField] private TurnController TurnController;

    void Start()
    {
        HexMapGenerator.GenerateMap();
        UnitsSpawner.Initialize(HexMapGenerator.LeftCity, HexMapGenerator.RightCity, HexMapGenerator.inlandHexes);
    }

    // Update is called once per frame
    void Update()
    {
        if(UnitsSpawner.FinishedSpawning && !TurnController.Initialized)
        {
            var leftPlayer = new PlayerData(Player.Left, HexMapGenerator.LeftCity, UnitsSpawner.LeftPlayerUnits, UnitsSpawner);
            var rightPlayer = new PlayerData(Player.Right, HexMapGenerator.RightCity, UnitsSpawner.RightPlayerUnits, UnitsSpawner);
            UnitsSpawner.FinishedSpawning = false;
            TurnController.Initialize(leftPlayer, rightPlayer, HexMapGenerator.inlandHexes);
        }

        if (TurnController.GameOver)
        {
            if (Input.GetKeyUp("return"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            
            if (Input.GetKeyUp("escape"))
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }
}
