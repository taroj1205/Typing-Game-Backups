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
    public static string historyPath = dataPath + "history.csv";
    public static string dictionaryPath = filesPath + "dictionary.csv";
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

    // AudioSource component to play the sound
    private AudioSource audioSource;

    private int _aNum;
    private string[] en;
    private string[] ja;
    private string[] history;
    private string[] parts;
    private string[] dic;
    private int randomIndex;
    private int words;

    // ゲームを始めた時に1度だけ呼ばれるもの
    void Awake()
    {
        Files();
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

    void KeyPressed()
    {
        Debug.Log(_eString);
        Debug.Log(_aNum);
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
            history = File.ReadAllLines(GlobalVariables.historyPath);
            string[] lines = File.ReadAllLines(GlobalVariables.historyPath);
            int words = lines.Length;
        }
    }

    void OutPut()
    {
        Files();

        _aNum = 0;

        var history = File.ReadAllLines(GlobalVariables.historyPath);
        string[] dictionary = File.ReadAllLines(GlobalVariables.dictionaryPath, Encoding.UTF8);
        string[] lines = File.ReadAllLines(GlobalVariables.historyPath);
        int words = lines.Length;

        // Split the dictionary string into two arrays, 'en' and 'ja'
        string[] en = new string[dictionary.Length];
        string[] ja = new string[dictionary.Length];
        for (int i = 0; i < dictionary.Length; i++)
        {
            string[] split = dictionary[i].Split(',');
            en[i] = split[0];
            ja[i] = split[1];
        }

        if (en.Length == ja.Length)
        {
            // Generate a random index between 0 and the length of the array
            int randomIndex = UnityEngine.Random.Range(0, dictionary.Length);

            Debug.Log(randomIndex);

            _eString = en[randomIndex];
            _jString = ja[randomIndex];

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

            // 文字を変更する
            jText.text = _jString;
            eText.text = _eString;
        }
        else
        {
            jText.text = "単語の数が合っていません";
            eText.text = "The amount of Vocabs does not match";
        }
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
            // 正解音
            CorrectSound();
            // 保存
            Save();
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