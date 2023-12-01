using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{

    public static GameObject Player   = GameObject.Find("Player");
    public static GameObject PlayerUI = GameObject.Find("PlayerUI");
    public static GameObject GameUI   = GameObject.Find("GameUI");
    public static GameObject Camera   = GameObject.Find("Camera");

    [SerializeField] private Text title_text;
    [SerializeField] private Text start_text;
    private bool textfade = false;
    private float fadeValue = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        if (textfade)
        {
            if (start_text.color.a >= 1.0f || start_text.color.a <= 0.2f)
            {
                fadeValue *= -1;
            }
            start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, start_text.color.a + fadeValue);
        }
    }

    private IEnumerator StartGame()
    {
        while (title_text.transform.position.y > 0) title_text.transform.position = new Vector2(title_text.transform.position.x, title_text.transform.position.y - 1);

        yield return new WaitForSeconds(0);
    }

}
