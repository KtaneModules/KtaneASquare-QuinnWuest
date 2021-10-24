using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ASquareScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMColorblindMode ColorblindMode;
    public GameObject ColorblindTextObj;
    public TextMesh ColorblindText;

    public KMSelectable SquareSel;
    public KMSelectable ModuleSel;
    public Renderer SquareObj;
    public Material[] SquareColors;
    public Material SquareWhite;
    public Material SquareCorrect;
    public Material SquareWrong;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;
    private bool _colorblindMode;

    private bool _isHeld;
    private bool _isStriking;

    private int _timerLastDigit;
    private int _timeIx;
    private int _currentColor;
    private int[] _correctColors = new int[3];
    private List<int> _inputColors = new List<int>();

    private int[] _colorShuffleArr = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private string[] COLORNAMES = { "Orange", "Pink", "Cyan", "Yellow", "Lavender", "Brown", "Tan", "Blue", "Jade", "Indigo", "White" };

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        _colorblindMode = ColorblindMode.ColorblindModeActive;
        SetColorblindMode(_colorblindMode);

        SquareSel.OnInteract += SquarePress;
        SquareSel.OnInteractEnded += SquareRelease;
        ModuleSel.OnFocus += ModuleFocus;
        ModuleSel.OnDefocus += ModuleDefocus;

    tryAgain:
        _colorShuffleArr.Shuffle();
        List<int> _indexColors = new List<int>();
        int score = 0;
        for (int i = 0; i < 10; i++)
            if (_colorShuffleArr[i] == i)
            {
                score++;
                _indexColors.Add(i);
            }
        if (score != 3)
            goto tryAgain;
        Debug.LogFormat("[A Square #{0}] Color corder: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}.", _moduleId,
            COLORNAMES[_colorShuffleArr[0]],
            COLORNAMES[_colorShuffleArr[1]],
            COLORNAMES[_colorShuffleArr[2]],
            COLORNAMES[_colorShuffleArr[3]],
            COLORNAMES[_colorShuffleArr[4]],
            COLORNAMES[_colorShuffleArr[5]],
            COLORNAMES[_colorShuffleArr[6]],
            COLORNAMES[_colorShuffleArr[7]],
            COLORNAMES[_colorShuffleArr[8]],
            COLORNAMES[_colorShuffleArr[9]]
            );

        Debug.LogFormat("[A Square #{0}] Index colors are: {1}, {2}, {3}.", _moduleId,
            COLORNAMES[_indexColors[0]],
            COLORNAMES[_indexColors[1]],
            COLORNAMES[_indexColors[2]]
            );

        //Calculate correct colors:
        if ((_indexColors[0] < 5 && _indexColors[1] < 5 && _indexColors[2] < 5) || (_indexColors[0] > 4 && _indexColors[1] > 4 && _indexColors[2] > 4))
        {
            _correctColors[0] = _indexColors[0];
            _correctColors[1] = _indexColors[1];
            _correctColors[2] = _indexColors[2];
            _correctColors.ToList().Sort();
            Debug.LogFormat("[A Square #{0}] A square's colors are all in the same row. Correct colors are {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
        else if (_indexColors[0] % 5 == _indexColors[1] % 5)
        {
            List<int> extraColors = new List<int>();
            for (int i = 0 + (_indexColors[2] > 4 ? 5 : 0); i < 5 + (_indexColors[2] > 4 ? 5 : 0); i++)
            {
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            }
            extraColors.Sort();
            _correctColors[0] = extraColors[2];
            _correctColors[1] = extraColors[1];
            _correctColors[2] = extraColors[0];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
        else if (_indexColors[0] % 5 == _indexColors[2] % 5)
        {
            List<int> extraColors = new List<int>();
            for (int i = 0 + (_indexColors[1] > 4 ? 5 : 0); i < 5 + (_indexColors[1] > 4 ? 5 : 0); i++)
            {
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            }
            extraColors.Sort();
            _correctColors[0] = extraColors[2];
            _correctColors[1] = extraColors[1];
            _correctColors[2] = extraColors[0];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
        else if (_indexColors[1] % 5 == _indexColors[2] % 5)
        {
            List<int> extraColors = new List<int>();
            for (int i = 0 + (_indexColors[0] > 4 ? 5 : 0); i < 5 + (_indexColors[0] > 4 ? 5 : 0); i++)
            {
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            }
            extraColors.Sort();
            _correctColors[0] = extraColors[2];
            _correctColors[1] = extraColors[1];
            _correctColors[2] = extraColors[0];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
        else if (!_indexColors.Contains(0) && !_indexColors.Contains(4) && !_indexColors.Contains(5) && !_indexColors.Contains(9))
        {
            List<int> extraColors = new List<int>();
            for (int i = 1; i < 4; i++)
            {
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
                if (!_indexColors.Contains(i + 5))
                    extraColors.Add(i + 5);
            }
            _correctColors[0] = extraColors[0];
            _correctColors[1] = extraColors[1];
            _correctColors[2] = extraColors[2];
            Debug.LogFormat("[A Square #{0}] None of a square's index colors are in the left or right columns. Correct colors are {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
        else
        {
            List<int> extraColors = new List<int>();
            if ((_indexColors[0] < 5 && _indexColors[1] < 5) || (_indexColors[0] > 4 && _indexColors[1] > 4))
            {
                _correctColors[0] = _indexColors[2];
                extraColors.Add(_indexColors[0]);
                extraColors.Add(_indexColors[1]);
                extraColors.Sort();
            }
            else if ((_indexColors[0] < 5 && _indexColors[2] < 5) || (_indexColors[0] > 4 && _indexColors[2] > 4))
            {
                _correctColors[0] = _indexColors[1];
                extraColors.Add(_indexColors[0]);
                extraColors.Add(_indexColors[2]);
                extraColors.Sort();
            }
            else if ((_indexColors[1] < 5 && _indexColors[2] < 5) || (_indexColors[1] > 4 && _indexColors[2] > 4))
            {
                _correctColors[0] = _indexColors[0];
                extraColors.Add(_indexColors[1]);
                extraColors.Add(_indexColors[2]);
                extraColors.Sort();
            }
            _correctColors[1] = extraColors[1];
            _correctColors[2] = extraColors[0];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors are in the same row. Correct colors are: {1}, {2}, {3}", _moduleId,
                COLORNAMES[_correctColors[0]],
                COLORNAMES[_correctColors[1]],
                COLORNAMES[_correctColors[2]]
                );
        }
    }

    private void SetColorblindMode(bool mode)
    {
        ColorblindTextObj.SetActive(mode);
    }

    private bool SquarePress()
    {
        Audio.PlaySoundAtTransform("MouseDown", transform);
        if (!_moduleSolved && !_isStriking)
        {
            _isHeld = true;
        }
        //Debug.LogFormat("[A Square #{0}] Press.", _moduleId);
        return false;
    }

    private void SquareRelease()
    {
        Audio.PlaySoundAtTransform("MouseUp", transform);
        if (!_moduleSolved && !_isStriking)
        {
            _isHeld = false;
        }
        if (_inputColors.Count == 3)
        {
            bool correct = true;
            for (int i = 0; i < 3; i++)
                if (_correctColors[i] != _inputColors[i])
                    correct = false;
            if (correct)
            {
                _moduleSolved = true;
                Module.HandlePass();
                Audio.PlaySoundAtTransform("Correct", transform);
                SquareObj.material = SquareCorrect;
                Debug.LogFormat("[A Square #{0}] Inputted {1}, {2}, {3}. Module solved.", _moduleId,
                    COLORNAMES[_correctColors[0]],
                    COLORNAMES[_correctColors[1]],
                    COLORNAMES[_correctColors[2]]
                );
            }
            else
            {
                Module.HandleStrike();
                SquareObj.material = SquareWrong;
                Debug.LogFormat("[A Square #{0}] Inputted {1}, {2}, {3} instead of {4}, {5}, {6}. Strike.", _moduleId,
                   COLORNAMES[_inputColors[0]],
                   COLORNAMES[_inputColors[1]],
                   COLORNAMES[_inputColors[2]],
                   COLORNAMES[_correctColors[0]],
                   COLORNAMES[_correctColors[1]],
                   COLORNAMES[_correctColors[2]]
               );
                StartCoroutine(WaitToChangeWhite());
            }
            ColorblindText.text = "";
            _inputColors = new List<int>();
        }
    }

    private void ModuleFocus()
    {
        if (!_moduleSolved && !_isStriking)
        {
            ColorblindText.text = COLORNAMES[_colorShuffleArr[_timerLastDigit]].ToUpper();
            SquareObj.material = SquareColors[_colorShuffleArr[_timerLastDigit]];
            _currentColor = _timerLastDigit;
        }
    }

    private void ModuleDefocus()
    {
        if (!_moduleSolved && !_isStriking)
        {
            ColorblindText.text = "WHITE";
            SquareObj.material = SquareWhite;
            _currentColor = 10;
        }
    }

    private void Update()
    {
        _timerLastDigit = (int)BombInfo.GetTime() % 10;
        if (_timeIx != _timerLastDigit)
        {
            _timeIx = _timerLastDigit;
            if (_isHeld)
            {
                Debug.LogFormat("[A Square #{0}] Held the color {1}.", _moduleId, COLORNAMES[_colorShuffleArr[_currentColor]]);
                _isHeld = false;
                _inputColors.Add(_colorShuffleArr[_currentColor]);
                
            }
        }
    }

    private IEnumerator WaitToChangeWhite()
    {
        _isStriking = true;
        yield return new WaitForSeconds(1.5f);
        _isStriking = false;
        SquareObj.material = SquareColors[_colorShuffleArr[_currentColor]];
        ColorblindText.text = COLORNAMES[_colorShuffleArr[_currentColor]].ToUpper();
    }
}
