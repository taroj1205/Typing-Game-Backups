using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System.Diagnostics;

public class GlobalVariables
{
    public static string currentDirectory = System.IO.Directory.GetCurrentDirectory();
    public static string dataPath = "Assets/Files/data/";
    public static string filesPath = "Assets/Files/";
    public static string historyPath = dataPath + "history.csv";
    public static string statsPath = dataPath + "stats.csv";
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
    [SerializeField] Text aText;

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
    int correct;
    int wrong;

    void Awake()
    {
        Files();
        ShowHistory();
        OutPut();
        ShowAccuracy();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Directory.Delete(GlobalVariables.dataPath, true);
                Awake();
                return;
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
            {
                System.Diagnostics.Process.Start("explorer.exe", Path.Combine(GlobalVariables.currentDirectory, GlobalVariables.filesPath.Replace('/', Path.DirectorySeparatorChar)));
                UnityEngine.Debug.Log(GlobalVariables.currentDirectory + Path.DirectorySeparatorChar + GlobalVariables.filesPath.Replace('/', Path.DirectorySeparatorChar));
                return;
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
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

        // Check if the stats file exists, create it if it doesn't
        if (!File.Exists(GlobalVariables.statsPath))
        {
            File.Create(GlobalVariables.statsPath).Dispose();
            File.WriteAllText(GlobalVariables.statsPath, "0,0", Encoding.UTF8);
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

            _eString = en[randomIndex];
            _jString = ja[randomIndex];

            // Check if the selected word exists in the first row of history.csv
            if (words > 0 && history[0].Contains(_eString))
            {
                _jString += " *";
            }

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

    void Correct()
    {
        _aNum++;
        // Read all the lines of the file into a string array
        string[] statsData = File.ReadAllLines(GlobalVariables.statsPath);

        // split the line with comma separator
        string[] parts = statsData[0].Split(',');
        // assign the first part as correct
        int correct = int.Parse(parts[0]);
        // assign the second part as wrong
        int wrong = int.Parse(parts[1]);

        correct++;
        
        string wCorrect = correct.ToString();
        string wWrong = wrong.ToString();

        string stats = wCorrect + "," + wWrong;
        File.WriteAllText(GlobalVariables.statsPath, stats, Encoding.UTF8);
        UnityEngine.Debug.Log("Correct! " + correct.ToString());
        ShowAccuracy();
        if (_aNum >= _eString.Length)
        {
            CorrectSound();
            Save();
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

    void Miss()
    {
        // Read all the lines of the file into a string array
        string[] statsData = File.ReadAllLines(GlobalVariables.statsPath);

        // split the line with comma separator
        string[] parts = statsData[0].Split(',');
        // assign the first part as correct
        int correct = int.Parse(parts[0]);
        // assign the second part as wrong
        int wrong = int.Parse(parts[1]);

        wrong++;

        string wCorrect = correct.ToString();
        string wWrong = wrong.ToString();

        string stats = wCorrect + "," + wWrong;
        File.WriteAllText(GlobalVariables.statsPath, stats, Encoding.UTF8);
        ShowAccuracy();
        string typedOut = "<color=grey>" + _eString.Substring(0, _aNum) + "</color>";
        string notYet = "<color=#e06c75>" + _eString.Substring(_aNum) + "</color>";
        eText.text = typedOut + notYet;
    }

    // 単語の履歴を保存
    void Save()
    {
        string eHistory = _eString;
        string jHistory = _jString;

        jHistory = jHistory.Replace(" *", "");

        string existingHistory = File.ReadAllText(GlobalVariables.historyPath);

        string history = eHistory + "," + jHistory;

        File.WriteAllText(GlobalVariables.historyPath, history + "\r\n" + existingHistory, Encoding.UTF8);
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
        string[] lines = File.ReadAllLines(GlobalVariables.historyPath);
        int words = lines.Length;
        if (words > 0)
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
        else if (words == 0)
        {
            hText.text = "Start Typing...";
        }
        else
        {
            hText.text = "Error...";
        }
    }

    void ShowAccuracy()
    {
        // Read all the lines of the file into a string array
        string[] statsData = File.ReadAllLines(GlobalVariables.statsPath);

        // split the line with comma separator
        string[] parts = statsData[0].Split(',');
        // assign the first part as correct
        int correct = int.Parse(parts[0]);
        // assign the second part as wrong
        int wrong = int.Parse(parts[1]);
        UnityEngine.Debug.Log(correct + "," + wrong);
        if (wrong == 0)
        {
            aText.text = "<color=#1fd755>" + "100%" + "</color>";
        }
        else if (correct == 0 && wrong > 0)
        {
            aText.text = "<color=#e06c75>" + "0%" + "</color>";
        }
        else
        {
            float accuracy = correct / (float)(correct + wrong) * 100.0f;
            accuracy = Mathf.Round(accuracy * 10f) / 10f;
            if (accuracy > 80)
            {
                string accuracyText = accuracy.ToString();
                aText.text = "<color=#1fd755>" + accuracyText + "%" + "</color>";
            }
            else if (accuracy <= 80)
            {
                string accuracyText = accuracy.ToString();
                aText.text = "<color=#e06c75>" + accuracyText + "%" + "</color>";
            }
        }
        string wCorrect = correct.ToString();
        string wWrong = wrong.ToString();

        string stats = wCorrect + "," + wWrong;
        File.WriteAllText(GlobalVariables.statsPath, stats, Encoding.UTF8);
    }
}