/* Основной класс управления игрой */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    [SerializeField] private Button pressToStart;
    [SerializeField] private GameObject[] gameMenuPanel;
    [SerializeField] private GameObject winMenuPanel;
    [SerializeField] private GameObject loseMenuPanel;

    //собранные монеты за один заход
    private int pointValue = 0;

    [Header("UIText")]
    private int pointCointCount = 0;
    [SerializeField] private RectTransform pointBarVisual;
    private float startPointVisualSize;

    //указываем количество собранных за заход монет на панелях победы/поражения
    [SerializeField] private Text winPointText;
    [SerializeField] private Text losePointText;

    //количество всех собранных монет за все время(правый верхний угол)
    [SerializeField] private Text allPointText;

    //указываем номер уровня при победе и поражении
    [SerializeField] private Text LoseLevelCountText;
    [SerializeField] private Text WinLevelCountText;

    //сколько монет осталось собрать для победы
    [SerializeField] private Text winNeedCountText;
    [SerializeField] private Text loseNeedCountText;

   // [Header("Sound")]
   // [SerializeField] private AudioClip backgroundClip;

    [HideInInspector] public bool gameIsPlayed = false;

    [Header("WinSetting")]
    private int winPointCount = 1;
    [SerializeField] private int nextLevelCountPlus;
    [SerializeField] private Text TimerText;
    [SerializeField] private float maxGameTime = 30.0f;
    private bool timeGo = false;
    private float time = 0;

    [Header("Next Level ID")]
    [SerializeField] private int nextLevelID = 1;

    [SerializeField] private ParticleSystem[] startParticals = new ParticleSystem[1];
    [SerializeField] private Animator startDoorsAnim;

    [Header("Режим для разработчиков! Обнуляет опыт")]
    [SerializeField] private bool ResetResult = false;

    [SerializeField] private GameObject AllCointVisual;
    [SerializeField] private Animator levelPointAnim;
    [SerializeField] private GameObject pointHalo;
    [SerializeField] private Animator pointAnim;
    [SerializeField] private Color32 timerColor;
    
    [Header("Events")]
    [SerializeField] private UnityEvent startGame = new UnityEvent();
    [SerializeField] private UnityEvent resetGame = new UnityEvent();
    [SerializeField] private UnityEvent endWinGame = new UnityEvent();
    [SerializeField] private UnityEvent endLoseGame = new UnityEvent();
    [SerializeField] private UnityEvent nextLevel = new UnityEvent();
    [SerializeField] private UnityEvent exitGame = new UnityEvent();

    [SerializeField] private UnityEvent nextLevelAnimPlay = new UnityEvent();

    private bool readyNextLevelGO = false;
    private bool useTimer = false;

    private void Awake()
    {
        if (ResetResult) PlayerPrefs.SetInt("WinPointCount", 200);

        instance = this;
        gameIsPlayed = false;
        pressToStart.gameObject.SetActive(true);
        readyNextLevelGO = false;
        TimerText.text = $"0:" + maxGameTime.ToString();

        startPointVisualSize = pointBarVisual.rect.width;//определяем 100% бара

        if (PlayerPrefs.HasKey("ALLCoints"))//набранные монеты за все время
        {
            allPointText.text = PlayerPrefs.GetInt("ALLCoints").ToString();
        }
        else
        {
            PlayerPrefs.SetInt("ALLCoints", 0);
            allPointText.text = PlayerPrefs.GetInt("ALLCoints").ToString();
        }

        if (PlayerPrefs.HasKey("UseLevel"))//количество уровней, пройденные игроком
        {
            WinLevelCountText.text = $"Level {PlayerPrefs.GetInt("UseLevel").ToString()}";
            LoseLevelCountText.text = $"Level {PlayerPrefs.GetInt("UseLevel").ToString()}";
        }
        else
        {
            PlayerPrefs.SetInt("UseLevel", 1);
            WinLevelCountText.text = $"Level {PlayerPrefs.GetInt("UseLevel").ToString()}";
            LoseLevelCountText.text = $"Level {PlayerPrefs.GetInt("UseLevel").ToString()}";
        }

        if (PlayerPrefs.HasKey("WinPointCount"))
            winPointCount = PlayerPrefs.GetInt("WinPointCount");

        if (PlayerPrefs.HasKey("LevelCoints"))//набранные монеты за уровень
        {
            pointCointCount = PlayerPrefs.GetInt("LevelCoints");
            float percent = (pointCointCount * 100) / winPointCount;
            pointBarVisual.sizeDelta = new Vector2(((startPointVisualSize * percent) / 100), pointBarVisual.rect.height);
        }
        else
        {
            PlayerPrefs.SetInt("LevelCoints", 0);
            pointCointCount = 0;

            float percent = (pointCointCount * 100) / winPointCount;
            pointBarVisual.sizeDelta = new Vector2(((startPointVisualSize * percent) / 100), pointBarVisual.rect.height);
        }
    }

public void StartGame()
    {
        for (int i = 0; i < gameMenuPanel.Length; i++)
            gameMenuPanel[i].SetActive(true);

        pressToStart.gameObject.SetActive(false);
        winMenuPanel.SetActive(false);
        loseMenuPanel.SetActive(false);
        AllCointVisual.SetActive(false);

        StartCoroutine(StartGameTime());
    }

    private IEnumerator StartGameTime()
    {
        yield return new WaitForSeconds(2f);

        startDoorsAnim.SetTrigger("GameStarted");

        gameIsPlayed = true;
        timeGo = true;
        TimerText.text = $"0:" + maxGameTime.ToString();
        startGame.Invoke();

        for (int i = 0; i < startParticals.Length; i++)
            startParticals[i].Play();
    }

    public void EndGame()
    {
        gameIsPlayed = false;
        timeGo = false;

        if (pointCointCount >= winPointCount)// если собранные за один заход монеты + за предыдущие заходы больше необходимого количества
        {
            for (int i = 0; i < gameMenuPanel.Length; i++)
                gameMenuPanel[i].SetActive(false);

            endWinGame.Invoke();
            StartCoroutine(WaitEndGamePanelActive(winMenuPanel, true));
        }
        else
        {
            for (int i = 0; i < gameMenuPanel.Length; i++)
                gameMenuPanel[i].SetActive(false);

            endLoseGame.Invoke();
            StartCoroutine(WaitEndGamePanelActive(loseMenuPanel, false));
        }
    }

    private IEnumerator WaitEndGamePanelActive(GameObject obj, bool win)
    {
        yield return new WaitForSeconds(1.5f);

        obj.SetActive(true);
        AllCointVisual.SetActive(true);

        if (win)
        {
            winPointText.text = pointValue.ToString();
            winNeedCountText.text = $"Need: {winPointCount.ToString()}";
        }
        else
        {
            losePointText.text = pointValue.ToString();
            loseNeedCountText.text = $"Need: {winPointCount.ToString()}";
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(UpdateALLCoints(win));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ResetGame()
    {
        PlayerPrefs.SetInt("ALLCoints", PlayerPrefs.GetInt("ALLCoints") + pointValue);
        PlayerPrefs.SetInt("LevelCoints", PlayerPrefs.GetInt("LevelCoints") + pointValue);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        PlayerPrefs.SetInt("WinPointCount", winPointCount + nextLevelCountPlus);
        PlayerPrefs.SetInt("UseLevel", PlayerPrefs.GetInt("UseLevel") + 1);
        PlayerPrefs.SetInt("ALLCoints", PlayerPrefs.GetInt("ALLCoints") + pointValue);
        PlayerPrefs.SetInt("LevelCoints", 0);

        StartCoroutine(EndGameAnimationPlay());
        //SceneManager.LoadScene(nextLevelID);
    }

    public void ReadyToGONextLevel()
    {
        readyNextLevelGO = true;
    }

    private IEnumerator EndGameAnimationPlay()
    {
        winMenuPanel.gameObject.SetActive(false);
        nextLevelAnimPlay.Invoke();

        while (!readyNextLevelGO)
            yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(nextLevelID);
    }

    private void FixedUpdate()
    {
        if (timeGo)
        {
            time += Time.deltaTime;

            if (Mathf.CeilToInt(maxGameTime - time) >= 10)
                TimerText.text = $"0:" + Mathf.CeilToInt(maxGameTime - time).ToString();
            else
                if (Mathf.CeilToInt(maxGameTime - time) == 5 && !useTimer)
                {
                    TimerText.text = $"0:0" + Mathf.CeilToInt(maxGameTime - time).ToString();
                    StartCoroutine(TimerAnim());
                }
                else
                    TimerText.text = $"0:0" + Mathf.CeilToInt(maxGameTime - time).ToString();

            if (time >= maxGameTime)
            {
                TimerText.text = "TIME IS UP";
                EndGame();
            }
        }
    }

    private IEnumerator TimerAnim()
    {
        useTimer = true;
        TimerText.color = timerColor;
        int i = 0;

        while (i < 5)
        {
            TimerText.fontSize = 90;
            yield return new WaitForSeconds(0.5f);

            TimerText.fontSize = 80;

            yield return new WaitForSeconds(0.5f);
            i++;
        }
    }

    public /*bool если нужно определить скидывать одежду или нет при победе*/ void UpdatePoint(int point)
    {
        pointValue += point;

        StartCoroutine(PointVisualUpdate(point));

       /* if ((pointValue + pointCointCount) >= winPointCount)
        {
            //EndGame();
            return true;
        }
        else
            return false;*/
    }

    private IEnumerator PointVisualUpdate(int point)
    {
        int count = point;

        while (count > 0)
        {
            pointCointCount += 1;
            count -= 1;

            float percent = (pointCointCount * 100) / winPointCount;
            pointBarVisual.sizeDelta = new Vector2(((startPointVisualSize * percent) / 100), pointBarVisual.rect.height);

            yield return new WaitForSeconds(0.005f);
        }

        //если достигли нужного количества, играем анимацию + запускаем свечение
        if (pointCointCount > winPointCount)
        {
            if (pointHalo != null) pointHalo.SetActive(true);
            if (levelPointAnim != null) levelPointAnim.SetTrigger("LevelUp");
        }
    }

    private IEnumerator UpdateALLCoints(bool win)
    {
        yield return new WaitForSeconds(1);

        int count = 0;

        if (win)
            count = int.Parse(winPointText.text);
        else
            count = int.Parse(losePointText.text);

        int allCount = int.Parse(allPointText.text);

        while (count > 0)
        {
            allCount += 1;
            count -= 1;

            allPointText.text = allCount.ToString();

            if (win)
                winPointText.text = count.ToString();
            else
                losePointText.text = count.ToString();

            if (pointAnim != null) pointAnim.SetTrigger("Active");

            yield return new WaitForSeconds(0.005f);
        }
    }
}
