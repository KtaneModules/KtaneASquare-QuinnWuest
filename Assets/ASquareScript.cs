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
    private bool _hasSubmitted;
    private List<int> _indexColors = new List<int>();

    private int _timerLastDigit;
    private int _timeIx;
    private int _currentColor;
    private readonly int[] _correctColors = new int[3];
    private List<int> _inputColors = new List<int>();

    private int[] _colorShuffleArr;
    private static readonly string[] _colorNames = { "Orange", "Pink", "Cyan", "Yellow", "Lavender", "Brown", "Tan", "Blue", "Jade", "Indigo", "White" };

    private bool _canChangeColors = true;
    private int _currentInputIx;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        _colorblindMode = ColorblindMode.ColorblindModeActive;
        SetColorblindMode(_colorblindMode);

        SquareSel.OnInteract += SquarePress;
        SquareSel.OnInteractEnded += SquareRelease;
        ModuleSel.OnHighlight += delegate { if (!TwitchPlaysActive) ChangeToColor(); };
        ModuleSel.OnHighlightEnded += delegate { if (!TwitchPlaysActive) ChangeToWhite(); };
        ModuleSel.OnFocus += delegate { _canChangeColors = false; };
        ModuleSel.OnDefocus += delegate { _canChangeColors = true; if (!TwitchPlaysActive) ChangeToWhite(); };

        do
            _colorShuffleArr = Enumerable.Range(0, 10).ToArray().Shuffle();
        while (Enumerable.Range(0, 10).Where(i => Array.IndexOf(_colorShuffleArr, i) == i).Count() != 3);
        _indexColors = Enumerable.Range(0, 10).Where(i => Array.IndexOf(_colorShuffleArr, i) == i).ToList();
        Debug.LogFormat("[A Square #{0}] Color order: {1}.", _moduleId, Enumerable.Range(0, 10).Select(i => _colorNames[_colorShuffleArr[i]]).Join(", "));
        Debug.LogFormat("[A Square #{0}] Index colors are: {1}, {2}, {3}.", _moduleId, _colorNames[_indexColors[0]], _colorNames[_indexColors[1]], _colorNames[_indexColors[2]]);

        //Calculate correct colors:
        if ((_indexColors[0] < 5 && _indexColors[1] < 5 && _indexColors[2] < 5) || (_indexColors[0] > 4 && _indexColors[1] > 4 && _indexColors[2] > 4))
        {
            for (int i = 0; i < 3; i++)
                _correctColors[i] = _indexColors[i];
            _correctColors.ToList().Sort();
            Debug.LogFormat("[A Square #{0}] A square's colors are all in the same row. Correct colors are {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
        }
        else if (_indexColors[0] % 5 == _indexColors[1] % 5)
        {
            var extraColors = new List<int>();
            for (int i = 0 + (_indexColors[2] > 4 ? 5 : 0); i < 5 + (_indexColors[2] > 4 ? 5 : 0); i++)
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            extraColors.Sort();
            for (int i = 0; i < 3; i++)
                _correctColors[i] = extraColors[2 - i];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
        }
        else if (_indexColors[0] % 5 == _indexColors[2] % 5)
        {
            var extraColors = new List<int>();
            for (int i = 0 + (_indexColors[1] > 4 ? 5 : 0); i < 5 + (_indexColors[1] > 4 ? 5 : 0); i++)
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            extraColors.Sort();
            for (int i = 0; i < 3; i++)
                _correctColors[i] = extraColors[2 - i];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
        }
        else if (_indexColors[1] % 5 == _indexColors[2] % 5)
        {
            var extraColors = new List<int>();
            for (int i = 0 + (_indexColors[0] > 4 ? 5 : 0); i < 5 + (_indexColors[0] > 4 ? 5 : 0); i++)
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
            extraColors.Sort();
            for (int i = 0; i < 3; i++)
                _correctColors[i] = extraColors[2 - i];
            Debug.LogFormat("[A Square #{0}] Two of a square's colors share a column. Correct colors are {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
        }
        else if (!_indexColors.Contains(0) && !_indexColors.Contains(4) && !_indexColors.Contains(5) && !_indexColors.Contains(9))
        {
            var extraColors = new List<int>();
            for (int i = 1; i < 4; i++)
            {
                if (!_indexColors.Contains(i))
                    extraColors.Add(i);
                if (!_indexColors.Contains(i + 5))
                    extraColors.Add(i + 5);
            }
            for (int i = 0; i < 3; i++)
                _correctColors[i] = extraColors[i];
            Debug.LogFormat("[A Square #{0}] None of a square's index colors are in the left or right columns. Correct colors are {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
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
            Debug.LogFormat("[A Square #{0}] Two of a square's colors are in the same row. Correct colors are: {1}.", _moduleId, Enumerable.Range(0, 3).Select(i => _colorNames[_correctColors[i]]).Join(", "));
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
            _isHeld = true;
        return false;
    }

    private void SquareRelease()
    {
        Audio.PlaySoundAtTransform("MouseUp", transform);
        if (!_moduleSolved && !_isStriking)
            _isHeld = false;
        if (!_hasSubmitted)
            return;
        _hasSubmitted = false;
        if (_inputColors[_currentInputIx] == _correctColors[_currentInputIx])
        {
            Debug.LogFormat("[A Square #{0}] Correctly inputted {1}.", _moduleId, _colorNames[_correctColors[_currentInputIx]]);
            _currentInputIx++;
            if (_currentInputIx == 3)
            {
                _moduleSolved = true;
                Module.HandlePass();
                ColorblindText.text = "GREEN";
                Audio.PlaySoundAtTransform("Correct", transform);
                SquareObj.material = SquareCorrect;
                Debug.LogFormat("[A Square #{0}] Module solved.", _moduleId);
                return;
            }
        }
        else
        {
            Module.HandleStrike();
            ColorblindText.text = "RED";
            SquareObj.material = SquareWrong;
            Debug.LogFormat("[A Square #{0}] Inputted {1} instead of {2}. Strike.", _moduleId, _colorNames[_inputColors[_currentInputIx]], _colorNames[_correctColors[_currentInputIx]]);
            _currentInputIx = 0;
            StartCoroutine(WaitToChangeWhite());
            _inputColors = new List<int>();
        }
    }

    private void ChangeToColor()
    {
        if (!_moduleSolved && !_isStriking && _canChangeColors)
        {
            Audio.PlaySoundAtTransform("Focus", transform);
            ColorblindText.text = _colorNames[_colorShuffleArr[_timerLastDigit]].ToUpper();
            SquareObj.material = SquareColors[_colorShuffleArr[_timerLastDigit]];
            _currentColor = _timerLastDigit;
        }
    }

    private void ChangeToWhite()
    {
        if (!_moduleSolved && !_isStriking && _canChangeColors)
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
                _isHeld = false;
                _inputColors.Add(_colorShuffleArr[_currentColor]);
                _hasSubmitted = true;
            }
        }
    }

    private IEnumerator WaitToChangeWhite()
    {
        _isStriking = true;
        yield return new WaitForSeconds(1.5f);
        _isStriking = false;
        if (!TwitchPlaysActive && !_canChangeColors)
        {
            SquareObj.material = SquareColors[_colorShuffleArr[_currentColor]];
            ColorblindText.text = _colorNames[_colorShuffleArr[_currentColor]].ToUpper();
        }
        else
        {
            SquareObj.material = SquareWhite;
            ColorblindText.text = "WHITE";
        }
    }

    private bool TwitchPlaysActive;
    private bool _canMakeNoise = true;
    private bool TwitchShouldCancelCommand;

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} focus 2 : Focus on a square on a 2. | !{0} submit 5 : Hold a square after focusing it on a 5. | !{0} cycle : Cycle through all colors. | !{0} colorblind : Set colorblind mode active.";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        int val;
        var m = Regex.Match(command, @"^\s*focus\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            if (!int.TryParse(m.Groups[1].Value, out val) || val < 0 || val > 9)
                yield break;
            _canChangeColors = false;
            yield return null;
            while (_timerLastDigit != val)
                yield return "trycancel";
            _canChangeColors = true;
            ChangeToColor();
            yield return new WaitForSeconds(1.5f);
            ChangeToWhite();
            yield break;
        }

        var n = Regex.Match(command, @"^\s*submit\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (n.Success)
        {
            if (!int.TryParse(n.Groups[1].Value, out val) || val < 0 || val > 9)
                yield break;
            _canChangeColors = false;
            yield return null;
            while (_timerLastDigit != val)
                yield return "trycancel";
            _canChangeColors = true;
            ChangeToColor();
            SquareSel.OnInteract();
            while (_timerLastDigit == val)
                yield return null;
            SquareSel.OnInteractEnded();
            ChangeToWhite();
            yield break;
        }

        var o = Regex.Match(command, @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (o.Success)
        {
            yield return null;
            _canMakeNoise = false;
            _canChangeColors = false;
            for (int i = 0; i < 11 && !TwitchShouldCancelCommand; i++)
            {
                val = _timerLastDigit;
                _canChangeColors = true;
                ChangeToColor();
                while (val == _timerLastDigit)
                    yield return null;
            }
            _canMakeNoise = true;
            ChangeToWhite();
            if (TwitchShouldCancelCommand)
                yield return "cancelled";
            yield break;
        }

        var p = Regex.Match(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (p.Success)
        {
            yield return null;
            SetColorblindMode(!_colorblindMode);
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = _currentInputIx; i < 3; i++)
        {
            while (Array.IndexOf(_colorShuffleArr, _correctColors[i]) != (int)BombInfo.GetTime() % 10)
                yield return true;
            ChangeToColor();
            yield return null;
            SquareSel.OnInteract();
            int t = (int)BombInfo.GetTime() % 10;
            while ((int)BombInfo.GetTime() % 10 == t)
                yield return null;
            SquareSel.OnInteractEnded();
            yield return null;
            ChangeToWhite();
        }
        yield break;
    }
}
