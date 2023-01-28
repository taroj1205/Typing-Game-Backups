using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

public class GlobalVariables
{
    public static string dataPath = "Assets/Files/data/";
    public static string filesPath = "Assets/Files/";
    public static string wordsPath = dataPath + "words.csv";
    public static string historyPath = dataPath + "history.csv";
    public static string dictionaryPath = filesPath + "dictionary.json";
    public static string translatedPath = filesPath + "translated.json";
}

// 画面にあるテキストの文字を変換したい
public class TypingManager : MonoBehaviour
{
    // 画面にあるテキストを持ってくる
    [SerializeField] Text jText;
    [SerializeField] Text eText;
    [SerializeField] Text hText;
    [SerializeField] Text wText;

    // 問題を用意しておく
    private string[] _japanese = { };
    private string[] _english = { };

    // 何番目か指定するためのstring
    private string _jString;
    private string _eString;
    private string _hString;
    private string _wString;

    // 何番目の問題か
    private int _qNum;

    // 問題の何文字目か
    private int _aNum;

    // AudioSource component to play the sound
    public AudioSource audioSource;

    string[] en;
    string[] ja;
    string[] history;
    string[] parts;
    int randomIndex;
    int words;

    // ゲームを始めた時に1度だけ呼ばれるもの
    void Awake()
    {
        Files();
        Read();
        ShowHistory();
        // 問題を出す
        OutPut();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            KeyPressed();
        }
    }

    void Read()
    {
        // Read all the lines of the file into a string array
        en = File.ReadAllLines(GlobalVariables.dictionaryPath);
        ja = File.ReadAllLines(GlobalVariables.translatedPath);
        history = File.ReadAllLines(GlobalVariables.historyPath);
    }

    void KeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Directory.Delete("Assets/files/data", true);
            Awake();
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Left Control!");
            Debug.Log("R!");
            OutPut();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            return;
        }
        else if (Input.GetKeyDown(_eString[_aNum].ToString()))
        {
            // 正解
            Correct();
        }
        else // if (Input.anyKeyDown)
        {
            // 失敗
            Miss();
        }
    }

    void Files()
    {
        // Check if the files directory exists, create if it doesn't
        if (!Directory.Exists(GlobalVariables.filesPath))
        {
            Directory.CreateDirectory(GlobalVariables.filesPath);
        }
        // Check if the data directory exists, create it if it doesn't
        if (!Directory.Exists(GlobalVariables.dataPath))
        {
            Directory.CreateDirectory(GlobalVariables.dataPath);
        }

        // Check if the history file exists, create it if it doesn't
        if (!File.Exists(GlobalVariables.historyPath))
        {
            File.Create(GlobalVariables.historyPath).Dispose();
        }
        if (File.Exists(GlobalVariables.historyPath))
        {
            string[] lines = File.ReadAllLines(GlobalVariables.historyPath);
            int words = lines.Length;
        }

        /*
        if (File.Exists(GlobalVariables.wordsPath))
        {
            // words = int.Parse(File.ReadAllLines(GlobalVariables.wordsPath)[0]);
        }

        // Check if the history file exists, create it if it doesn't
        if (!File.Exists(GlobalVariables.wordsPath))
        {
            File.Create(GlobalVariables.wordsPath).Dispose();
        }*/
    }

    void OutPut()
    {
        Files();

        // Generate a random index between 0 and the length of the array
        int randomIndex = UnityEngine.Random.Range(0, en.Length);

        var history = File.ReadAllLines(GlobalVariables.historyPath);
        string[] lines = File.ReadAllLines(GlobalVariables.historyPath);
        int words = lines.Length;

        string question;
        string answer;

        question = en[randomIndex];
        answer = ja[randomIndex];

        _eString = question;
        _jString = answer;

        if (words > 0)
        {
            string[] wLines = File.ReadAllLines(GlobalVariables.historyPath);
            int word_count = wLines.Length;
            _wString = string.Concat(word_count);
        }
        else
        {
            _wString = "0";
        }

        // 0番目に文字を戻す
        _aNum = 0;

        // 文字を変更する
        jText.text = _jString;
        eText.text = _eString;
        wText.text = "Words: " + _wString;
    }

    // 正解用の関数
    void Correct()
    {
        // 正解したときの処理（やりたいこと）
        _aNum++;
        Debug.Log("正解したよ！");

        // 最後の文字に正解したら
        if (_aNum >= _eString.Length)
        {
            words++;
            Debug.Log(words);
            // 正解音
            CorrectSound();
            // 保存
            Save();
            eText.text = ja[randomIndex];
            jText.text = "";
            // 問題を変える
            OutPut();
            ShowHistory();
        }
        else
        {
            string typedOut = "<color=grey>" + _eString.Substring(0, _aNum) + "</color>";
            string notYet = "<color=#1fd755>" + _eString.Substring(_aNum) + "</color>";
            eText.text = typedOut + notYet;
        }
    }

    // 間違え用の関数
    void Miss()
    {
        string typedOut = "<color=grey>" + _eString.Substring(0, _aNum) + "</color>";
        string notYet = "<color=#e06c75>" + _eString.Substring(_aNum) + "</color>";
        eText.text = typedOut + notYet;

        // 間違えたときにやりたいこと
        Debug.Log("間違えたよ！");
    }

    // 単語の履歴を保存
    void Save()
    {
        string eHistory = _eString;
        string jHistory = _jString;

        string existingHistory = File.ReadAllText(GlobalVariables.historyPath);

        // create a new string variable that contains jstring and estring 
        string history = eHistory + "," + jHistory;
        // Append the new history to the existing content of the file
        File.WriteAllText(GlobalVariables.historyPath, history + Environment.NewLine + existingHistory, Encoding.Unicode);
    }

    void CorrectSound()
    {
        // Find the AudioSource component on the GameObject
        AudioSource audioSource = GetComponent<AudioSource>();
        // Play the sound
        audioSource.Play();
    }

    void ShowHistory()
    {
        // Check if the history file exists
        if (GlobalVariables.historyPath.Length > 0)
        {
            // Read all the lines of the file into a string array
            string[] history = File.ReadAllLines(GlobalVariables.historyPath);

            // Clear the hText UI element
            hText.text = "";

            // Iterate through the array and add each line to the hText UI element
            for (int i = 0; i < history.Length; i++)
            {
                // split the line with comma separator
                string[] parts = history[i].Split(',');
                // assign the first part as jstring
                string estring = parts[0];
                // assign the second part as estring
                string jstring = parts[1];
                // join estring and jstring with colon separator
                string line = estring + ": " + jstring;
                if (i == 0)
                {
                    hText.text += line + " <" + "\n";
                }
                else
                {
                    hText.text += line + "\n";
                }
            }

        }
        else
        {
            hText.text = "Start Typing...";
        }
    }
}
