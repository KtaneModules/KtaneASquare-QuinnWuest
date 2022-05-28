using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private List<int> _indexColors = new List<int>();

    private int _timerLastDigit;
    private int _timeIx;
    private int _currentColor;
    private int[] _correctColors = new int[3];
    private List<int> _inputColors = new List<int>();

    private int[] _colorShuffleArr = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private string[] COLORNAMES = { "Orange", "Pink", "Cyan", "Yellow", "Lavender", "Brown", "Tan", "Blue", "Jade", "Indigo", "White" };

    private bool TwitchPlaysActive;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        _colorblindMode = ColorblindMode.ColorblindModeActive;
        SetColorblindMode(_colorblindMode);

        SquareSel.OnInteract += SquarePress;
        SquareSel.OnInteractEnded += SquareRelease;
        ModuleSel.OnFocus += delegate { ModuleFocus(!TwitchPlaysActive); };
        ModuleSel.OnDefocus += delegate { ModuleDefocus(!TwitchPlaysActive); };

        tryAgain:
        _colorShuffleArr.Shuffle();
        _indexColors = new List<int>();
        int score = 0;
        for (int i = 0; i < 10; i++)
            if (_colorShuffleArr[i] == i)
            {
                score++;
                _indexColors.Add(i);
            }
        if (score != 3)
            goto tryAgain;
        Debug.LogFormat("[A Square #{0}] Color order: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}.", _moduleId,
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
        _colorblindMode = mode;
        ColorblindTextObj.SetActive(_colorblindMode);
    }

    private bool SquarePress()
    {
        Audio.PlaySoundAtTransform("MouseDown", transform);
        if (!_moduleSolved && !_isStriking)
        {
            _isHeld = true;
        }

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

    private void ModuleFocus(bool auto = true)
    {
        if (!_moduleSolved && !_isStriking && auto)
        {
            Audio.PlaySoundAtTransform("Focus", transform);
            ColorblindText.text = COLORNAMES[_colorShuffleArr[_timerLastDigit]].ToUpper();
            SquareObj.material = SquareColors[_colorShuffleArr[_timerLastDigit]];
            _currentColor = _timerLastDigit;
        }
    }

    private void ModuleDefocus(bool auto = true)
    {
        if (!_moduleSolved && !_isStriking && auto)
        {
            if (_canMakeNoise)
                Audio.PlaySoundAtTransform("Defocus", transform);
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
                Audio.PlaySoundAtTransform("Input", transform);
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
        if (!TwitchPlaysActive)
        {
            SquareObj.material = SquareColors[_colorShuffleArr[_currentColor]];
            ColorblindText.text = COLORNAMES[_colorShuffleArr[_currentColor]].ToUpper();
        }
        else
        {
            SquareObj.material = SquareWhite;
            ColorblindText.text = "WHITE";
        }
    }

    private bool _canMakeNoise = true;

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} focus 2 : Focus on a square on a 2. | !{0} submit 5 : Hold a square after focusing it on a 5. | !{0} cycle : Cycle through all colors. | !{0} colorblind : Set colorblind mode active.";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        int val;
        var m = Regex.Match(command, @"^\s*(?:focus\s+)?(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            if (!int.TryParse(m.Groups[1].Value, out val) || val < 0 || val > 9)
                yield break;
            yield return null;
            while (_timerLastDigit != val)
                yield return null;
            ModuleFocus();
            yield return new WaitForSeconds(1.5f);
            ModuleDefocus();
        }

        var n = Regex.Match(command, @"^\s*(?:submit\s+)?(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (n.Success)
        {
            if (!int.TryParse(n.Groups[1].Value, out val) || val < 0 || val > 9)
                yield break;
            yield return null;
            while (_timerLastDigit != val)
                yield return null;
            ModuleFocus();
            //yield return new WaitForSeconds(0.1f);
            SquareSel.OnInteract();
            while (_timerLastDigit == val)
                yield return null;
            SquareSel.OnInteractEnded();
            //yield return new WaitForSeconds(0.1f);
            ModuleDefocus();
        }

        var o = Regex.Match(command, @"^\s*(?:cycle)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (o.Success)
        {
            yield return null;
            _canMakeNoise = false;
            for (int i = 0; i < 11; i++)
            {
                val = _timerLastDigit;
                ModuleFocus();
                while (val == _timerLastDigit)
                    yield return "trycancel";
                ModuleDefocus();
            }
            _canMakeNoise = true;
        }

        var p = Regex.Match(command, @"^\s*(?:colorblind)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (p.Success)
        {
            yield return null;
            SetColorblindMode(!_colorblindMode);
        }
        yield break;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = 0; i < _inputColors.Count; i++)
            if (_inputColors[i] != _correctColors[i])
            {
                _inputColors = new List<int>();
                Debug.LogFormat("[A Square #{0}] An incorrect submission has been detected while executing the TP autosolver. Resetting input to avoid strike.", _moduleId);
                // There's a bit of a debate on what to do in the case of an autosolver being run while the module has reached a state where a strike is unavoidable.
                // I, however, have the stance that modules should never enter such a state, and should strike anyway if such state is detected. (Refer to X01)
                // However, I've come to this stance after developing this module, and I can't really go backwards...??
                // I much prefer that autosolvers appear to solve the same way as a human solves it, regardless if internal data needs to change,
                // whereas autosolver developers such as eXish believe that the module should solve immediately, so internal data doesn't get changed.
            }
        for (int i = _inputColors.Count; i < 3; i++)
        {
            while (Array.IndexOf(_colorShuffleArr, _correctColors[i]) != (int)BombInfo.GetTime() % 10)
                yield return true;
            ModuleFocus();
            yield return null;
            SquareSel.OnInteract();
            int t = (int)BombInfo.GetTime() % 10;
            while ((int)BombInfo.GetTime() % 10 == t)
                yield return null;
            SquareSel.OnInteractEnded();
            yield return null;
            ModuleDefocus();
        }
        yield break;
    }
}
