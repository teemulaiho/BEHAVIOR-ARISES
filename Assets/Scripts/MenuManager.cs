using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    static MenuManager instance;
    Scene currentScene;
    public List<GameObject> moshbots;
    public List<Button> buttons;

    public Animator transition;
    public float transitionTime = 1f;

    public bool allowInput = false;

    private void OnEnable()
    {

    }

    private void Awake()
    {
        CheckForDuplicateGameObjects();     // Remove DontDestroyOnLoad Duplicates.
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(CheckForSceneChanges())
        {
            GetUIButtonsFromActiveScene();
        }

        if (allowInput)
            GetInput();
    }

    private bool CheckForSceneChanges()
    {
        if (currentScene != SceneManager.GetActiveScene())
        {
            currentScene = SceneManager.GetActiveScene();
            return true;
        }
        else
            return false;
    }

    private void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.P))
        {
            if (SceneManager.GetActiveScene().name != "MainMenu" &&
                SceneManager.GetActiveScene().name != "MoshbotViewer")
            {
                //SceneManager.LoadScene("MainMenu");
                LoadSceneMainMenu();
            }
        }
    }

    private void GetUIButtonsFromActiveScene()
    {
        buttons.Clear();
        buttons.AddRange(FindObjectsOfType<Button>());

        if (buttons.Count > 0)
        {
            foreach (Button b in buttons)
            {
                Debug.Log(b.name);
                SetButtonFunctions(b);
            }
        }
    }

    private void SetButtonFunctions(Button b)
    {
        if (b.name == "Play")
        {
            b.onClick.AddListener(LoadGameScene);
        }
        else if (b.name == "Choose Moshbot")
        {
            b.onClick.AddListener(LoadSceneMoshBotViewer);
        }
        else if (b.name == "Main Menu")
        {
            b.onClick.AddListener(LoadSceneMainMenu);
        }
    }

    private void CheckForDuplicateGameObjects()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    public void LoadGameScene()
    {
        //SceneManager.LoadScene("Level 1");
        StartCoroutine(LoadScene("Level 1"));
    }

    public void LoadSceneMainMenu()
    {
        //SceneManager.LoadScene("MainMenu");
        StartCoroutine(LoadScene("MainMenu"));
    }

    public void LoadSceneMoshBotViewer()
    {
        //SceneManager.LoadScene("MoshbotViewer");
        StartCoroutine(LoadScene("MoshbotViewer"));
    }

    IEnumerator LoadScene(string level)
    {
        transition.SetTrigger("Start");
        allowInput = false;

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);

        transition.SetTrigger("End");

        yield return new WaitForSeconds(transitionTime);
        allowInput = true;
    }
}
