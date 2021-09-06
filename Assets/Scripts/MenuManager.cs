using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    
    public List<GameObject> moshbots;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSceneMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSceneMoshBotViewer()
    {
        SceneManager.LoadScene("Moshbotviewer");
    }
}
