using System.Text;
using UnityEngine;

namespace Framework.Runtime.Utilities
{
  public static class TimeFormat
  {
    private const string ZeroTimeString = "00:00";
    private const string ZeroString = "0";
    private const int MaxCounter = 3;
    private static readonly string[] _withNumbers = new string[61];
    private static readonly string[] _labels = new string[5];
    private static readonly StringBuilder _stringBuilder = new StringBuilder();

    public static string Format(float timeInSeconds, bool isDynamic = false, bool withNull = true,
            bool withSeconds = true, bool withChars = true, bool withZero = true)
    {
      if (timeInSeconds < 0 || float.IsNaN(timeInSeconds)) return isDynamic ? ZeroTimeString : string.Empty;

      if (_withNumbers[0] == null)
      {
        for (int i = 0; i < _withNumbers.Length; i++)
        {
          _withNumbers[i] = i.ToString();
        }
      }

      int denominator = 60 * 60 * 24 * 7;

      double nRemainder = timeInSeconds / denominator;
      int nWeeks = (int)nRemainder;
      timeInSeconds -= (nWeeks * denominator);

      denominator = 60 * 60 * 24;

      nRemainder = timeInSeconds / denominator;
      int nDays = (int)nRemainder;
      timeInSeconds -= (nDays * denominator);

      denominator = 60 * 60;

      nRemainder = timeInSeconds / denominator;
      int nHours = (int)nRemainder;
      timeInSeconds -= (nHours * denominator);

      denominator = 60;

      nRemainder = timeInSeconds / denominator;
      int nMinutes = (int)nRemainder;
      timeInSeconds -= nMinutes * denominator;

      int nSeconds = (int)timeInSeconds;

      if (nWeeks == 0 && nDays == 0 && nHours == 0 && nMinutes == 0)
      {
        nSeconds = Mathf.FloorToInt(timeInSeconds);
      }

      if (withSeconds == false) nSeconds = 0;

      _labels[0] = "w";//ScriptLocalization.W;
      _labels[1] = "d";//ScriptLocalization.D;
      _labels[2] = "h";//ScriptLocalization.H;
      _labels[3] = "m";//ScriptLocalization.M;
      _labels[4] = "s";//ScriptLocalization.S;


      string w = withChars ? _labels[0] : string.Empty;
      string d = withChars ? _labels[1] : string.Empty;
      string h = withChars ? _labels[2] : string.Empty;
      string m = withChars ? _labels[3] : string.Empty;
      string s = withChars ? _labels[4] : string.Empty;

      string wString = (nWeeks >= _withNumbers.Length) ? nWeeks.ToString() : _withNumbers[nWeeks];
      string dString = (nDays >= _withNumbers.Length) ? nDays.ToString() : _withNumbers[nDays];
      string hString = _withNumbers[nHours];
      string mString = _withNumbers[nMinutes];
      string sString = _withNumbers[nSeconds];

      int counter = 0;

      _stringBuilder.Length = 0;

      if (isDynamic)
      {
        if (withSeconds)
        {
          if (nWeeks == 0 && nDays == 0 && nHours == 0 && nMinutes == 0)
          {
            return sString + s;
          }
          if (nWeeks == 0 && nDays == 0 && nHours == 0)
          {
            _stringBuilder.Append(mString);
            _stringBuilder.Append(m);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(sString);
            _stringBuilder.Append(s);
            return _stringBuilder.ToString();
          }
          if (nWeeks == 0 && nDays == 0 && nHours == 0)
          {
            _stringBuilder.Append(mString);
            _stringBuilder.Append(m);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(sString);
            _stringBuilder.Append(s);
            return _stringBuilder.ToString();
          }
          if (nWeeks == 0 && nDays == 0)
          {
            _stringBuilder.Append(hString);
            _stringBuilder.Append(h);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(mString);
            _stringBuilder.Append(m);
            return _stringBuilder.ToString();
          }
          if (nWeeks == 0)
          {
            _stringBuilder.Append(dString);
            _stringBuilder.Append(d);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(hString);
            _stringBuilder.Append(h);
            return _stringBuilder.ToString();
          }
          _stringBuilder.Append(wString);
          _stringBuilder.Append(w);
          _stringBuilder.Append(' ');
          _stringBuilder.Append(dString);
          _stringBuilder.Append(d);

          return _stringBuilder.ToString();
        }
        else
        {
          if (nWeeks == 0 && nDays == 0 && nHours == 0 && nMinutes == 0)
          {
            return ZeroString;
          }
          if (nWeeks == 0 && nDays == 0 && nHours == 0)
          {
            return mString + m;
          }
          if (nWeeks == 0 && nDays == 0)
          {
            _stringBuilder.Append(hString);
            _stringBuilder.Append(h);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(mString);
            _stringBuilder.Append(m);
            return _stringBuilder.ToString();
          }
          if (nWeeks == 0)
          {
            _stringBuilder.Append(dString);
            _stringBuilder.Append(d);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(hString);
            _stringBuilder.Append(h);
            return _stringBuilder.ToString();
          }
          _stringBuilder.Append(wString);
          _stringBuilder.Append(w);
          _stringBuilder.Append(' ');
          _stringBuilder.Append(dString);
          _stringBuilder.Append(d);

          return _stringBuilder.ToString();
        }
      }
      else
      {
        if (nWeeks != 0)
        {
          if (_stringBuilder.Length != 0) _stringBuilder.Append(' ');
          _stringBuilder.Append(wString);
          _stringBuilder.Append(w);
          counter++;
        }
        if (nDays != 0)
        {
          if (_stringBuilder.Length != 0)
          {
            _stringBuilder.Append(' ');
          }
          _stringBuilder.Append(dString);
          _stringBuilder.Append(d);
          counter++;
        }
        if (nHours != 0)
        {
          if (_stringBuilder.Length != 0)
          {
            _stringBuilder.Append(' ');
          }
          _stringBuilder.Append(hString);
          _stringBuilder.Append(h);
          counter++;
          if (counter == MaxCounter) return _stringBuilder.ToString();
        }
        if (nMinutes != 0)
        {
          if (_stringBuilder.Length != 0)
          {
            _stringBuilder.Append(' ');
            if (withZero && nMinutes < 10)
            {
              _stringBuilder.Append('0');
            }
          }
          _stringBuilder.Append(mString);
          _stringBuilder.Append(m);
          counter++;
          if (counter == MaxCounter) return _stringBuilder.ToString();
        }
        if (nSeconds != 0 || _stringBuilder.Length == 0)
        {
          if (_stringBuilder.Length != 0)
          {
            _stringBuilder.Append(' ');
            if (withZero && nSeconds < 10)
            {
              _stringBuilder.Append('0');
            }
          }
          _stringBuilder.Append(sString);
          _stringBuilder.Append(s);
          counter++;
          if (counter == MaxCounter) return _stringBuilder.ToString();
        }

        return _stringBuilder.ToString();
      }
    }
  }
}
