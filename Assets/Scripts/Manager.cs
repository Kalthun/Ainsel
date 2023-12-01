using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{

    private GameObject Title_Text;
    private Text start_text;
    private bool slowDrop = false;
    private bool textfade = false;
    private float fadeValue = 0.01f;

    private int sceneIndex = 0; // 0 for title

    // Start is called before the first frame update
    void Start()
    {

        Title_Text = GameObject.Find("Title_Text");
        start_text = GameObject.Find("start_text").GetComponent<Text>();
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 0);

        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {

        if (slowDrop)
        {
            Title_Text.transform.position = new Vector2(Title_Text.transform.position.x, Title_Text.transform.position.y - 1);
        }

        if (textfade)
        {
            if (start_text.color.a >= 1.0f || start_text.color.a <= 0.3f)
            {
                fadeValue *= -1;
            }
            start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, start_text.color.a + fadeValue);
        }
    }

    private IEnumerator StartGame()
    {
        slowDrop = true;
        yield return new WaitUntil(() => Title_Text.transform.position.y < 0);
        slowDrop = false;
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, 1f);
        textfade = true;
    }

    private void LoadNext()
    {

    }

}
