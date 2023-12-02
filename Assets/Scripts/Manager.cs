using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{

    private int sceneIndex = 0;
    private List<string> scenes = new List<string>();

    private GameObject Camera;
    private GameObject Parallax;
    private GameObject PlayerPrefab;
    private GameObject Player;
    public bool respawning = false;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] sounds;

    private bool firstLoad = true;

    public bool isPaused = false;
    [SerializeField] private GameObject PauseCanvas;
    [SerializeField] private Texture2D cursor;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private Dropdown resolutionSelection;
    private List<int> widths;
    private List<int> heights;

    private GameObject Title_Text;
    private Text start_text;
    private bool slowDrop = false;
    private bool textfade = false;
    private float fadeValue = 0.01f;

    // Start is called before the first frame update
    void Start()
    {

        widths = new List<int>() {1920, 1280};
        heights = new List<int>() {1080, 720};

        Camera = GameObject.Find("Camera");
        Object.DontDestroyOnLoad(Camera);

        Parallax = GameObject.Find("Parallax");
        Object.DontDestroyOnLoad(Parallax);
        Parallax.SetActive(false);

        PlayerPrefab = GameObject.Find("PlayerPrefab");
        Player = GameObject.Find("Player");
        Object.DontDestroyOnLoad(PlayerPrefab);
        PlayerPrefab.SetActive(false);

        scenes.Add("Title"); // 0
        scenes.Add("Level1"); // 1
        scenes.Add("Level2"); // 2
        scenes.Add("Level3"); // 3

        source.loop = true; // repeat music

        Title_Text = GameObject.Find("Title_Text");
        start_text = GameObject.Find("start_text").GetComponent<Text>();
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 0);

        StartCoroutine(TitleStart());
    }

    // Update is called once per frame
    void Update()
    {

        CheckPause();

        Debug.Log(Screen.fullScreenMode);

        if (isPaused) return;

        switch (sceneIndex)
        {
            case 0:
                Title();
            break;

            case 1:
                Level1();
            break;

            case 2:
                Level2();
            break;

            case 3:
                Level3();
            break;

            default:
            break;
        }

    }

    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();
        }

       if (isPaused)
       {
        source.volume = volumeSlider.value;
        Screen.SetResolution(widths[resolutionSelection.value], heights[resolutionSelection.value], fullscreenToggle.isOn);
       }
    }


    private void TogglePause()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;
            PauseCanvas.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            isPaused = true;
            PauseCanvas.SetActive(true);
        }
    }

    public void Spawn()
    {
        Player.transform.position = GameObject.Find("SpawnPoint").transform.position;
    }

    public void LoadNext()
    {
        if (sceneIndex == 3)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            sceneIndex++;
            SceneManager.LoadScene(scenes[sceneIndex]);
            firstLoad = true;
        }
    }

    private void Title()
    {
        if (slowDrop)
        {
            Title_Text.transform.position = new Vector2(Title_Text.transform.position.x, Title_Text.transform.position.y - fadeValue);
        }

        if (textfade)
        {
            if (start_text.color.a >= 1f || start_text.color.a <= 0.3f)
            {
                fadeValue *= -1;
            }
            start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, start_text.color.a + fadeValue);
        }

        if (Input.GetMouseButtonUp(0) && !isPaused)
        {
            LoadNext();
        }
    }

    private IEnumerator TitleStart()
    {
        source.clip = sounds[0];
        source.Play();
        slowDrop = true;
        yield return new WaitUntil(() => Title_Text.transform.position.y < 0 || sceneIndex != 0);
        slowDrop = false;
        if (sceneIndex == 0) start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 1f);
        textfade = true;
    }

    private void Level1()
    {

        if (firstLoad)
        {
            Parallax.SetActive(true);
            PlayerPrefab.SetActive(true);
            source.clip = sounds[1];
            source.Play();
            Spawn();
            firstLoad = false;
        }

    }

    private void Level2()
    {
        if (firstLoad)
        {
            Spawn();
            firstLoad = false;
        }
    }

    private void Level3()
    {
        if (firstLoad)
        {
            Spawn();
            firstLoad = false;
        }
    }

}
