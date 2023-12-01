using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{

    private int sceneIndex = -1; // 0 for pause
    private List<string> scenes = new List<string>();

    private GameObject CameraPrefab;
    private GameObject ParallaxPrefab;
    private GameObject PlayerPrefab;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] sounds;

    private GameObject Title_Text;
    private Text start_text;
    private bool slowDrop = false;
    private bool textfade = false;
    private float fadeValue = 0.01f;



    // Start is called before the first frame update
    void Start()
    {

        CameraPrefab = GameObject.Find("Camera");
        Object.DontDestroyOnLoad(CameraPrefab);

        ParallaxPrefab = GameObject.Find("Parallax");
        Object.DontDestroyOnLoad(ParallaxPrefab);
        ParallaxPrefab.SetActive(false);

        PlayerPrefab = GameObject.Find("PlayerPrefab");
        Object.DontDestroyOnLoad(PlayerPrefab);
        PlayerPrefab.SetActive(false);

        scenes.Add("Pause");
        scenes.Add("Menu");
        scenes.Add("Testing Ground");

        Title_Text = GameObject.Find("Title_Text");
        start_text = GameObject.Find("start_text").GetComponent<Text>();
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 0);

        StartCoroutine(TitleStart());
    }

    // Update is called once per frame
    void Update()
    {

        switch (sceneIndex)
        {
            case -1:
                Title();
            break;

            case 1:
                Menu();
            break;

            default:
            break;
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

        if (Input.GetMouseButtonUp(0) && !slowDrop)
        {
            sceneIndex = 1;
            SceneManager.LoadScene(scenes[sceneIndex]);
        }
    }

    private void Menu()
    {

    }

    private IEnumerator TitleStart()
    {
        source.clip = sounds[0];
        source.Play();
        slowDrop = true;
        yield return new WaitUntil(() => Title_Text.transform.position.y < 0);
        slowDrop = false;
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 1f);
        textfade = true;
    }



}
