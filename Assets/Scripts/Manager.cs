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
    [SerializeField] Collider2D playerHitbox;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] sounds;

    private bool firstLoad = true;

    private GameObject Title_Text;
    private Text start_text;
    private bool slowDrop = false;
    private bool textfade = false;
    private float fadeValue = 0.01f;

    // Start is called before the first frame update
    void Start()
    {

        Camera = GameObject.Find("Camera");
        Object.DontDestroyOnLoad(Camera);

        Parallax = GameObject.Find("Parallax");
        Object.DontDestroyOnLoad(Parallax);
        Parallax.SetActive(false);

        PlayerPrefab = GameObject.Find("PlayerPrefab");
        Player = GameObject.Find("Player");
        Object.DontDestroyOnLoad(PlayerPrefab);
        PlayerPrefab.SetActive(false);

        scenes.Add("Title");  // 0
        scenes.Add("Testing Ground"); // 1
        scenes.Add("Template"); // 2
        scenes.Add("Level3"); // 3
        scenes.Add("Pause");  // 4

        source.loop = true; // repeat music

        Title_Text = GameObject.Find("Title_Text");
        start_text = GameObject.Find("start_text").GetComponent<Text>();
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 0);

        StartCoroutine(TitleStart());
    }

    // Update is called once per frame
    void Update()
    {

        checkPause();

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

            case 4:
                Pause();
            break;

            default:
            break;
        }

        checkFall();

    }

    private void checkPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

        }
    }

    private void checkFall()
    {
        if (Player.transform.position.y < -20)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        Player.transform.position = GameObject.Find("SpawnPoint").transform.position;
    }

    public void LoadNext()
    {
        sceneIndex++;
        SceneManager.LoadScene(scenes[sceneIndex]);
        firstLoad = true;
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

        if (Input.GetMouseButtonUp(0))
        {
            Parallax.SetActive(true);
            PlayerPrefab.SetActive(true);
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

    private void Pause()
    {

    }

}
