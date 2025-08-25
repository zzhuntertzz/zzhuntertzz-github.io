using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Security.Cryptography;
using System.IO;
using Lean.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class FunctionCommon
{
    public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    private static System.Random rng = new System.Random();

    public static readonly string COLOR_HEX_BLUE = "#04B1FF";
    public static readonly string COLOR_HEX_GREEN = "#67D22E";
    public static readonly string COLOR_HEX_RED = "#DD1818";

    #region Data

    public static void SaveData<T>(this object obj)
    {
        PlayerPrefs.SetString(typeof(T).Name, JsonConvert.SerializeObject(obj));
        // Debug.Log($">>>{typeof(T).Name}: {JsonConvert.SerializeObject(obj)}");
    }
    
    public static T LoadData<T>(Action<T> onNew = null, Action<T> onLoaded = null) where T : new()
    {
        var json = PlayerPrefs.GetString(typeof(T).Name);
        T data;
        if (string.IsNullOrEmpty(json))
        {
            data = new T();
            onNew?.Invoke(data);
        }
        else
        {
            data = JsonConvert.DeserializeObject<T>(json);
        }
        
        onLoaded?.Invoke(data);

        return data;
    }
    
    public static bool HasData<T>() where T : new()
    {
        var json = PlayerPrefs.GetString(typeof(T).Name);
        return !string.IsNullOrEmpty(json);
    }

    #endregion

    #region Cam

    static Camera _cam;
    public static Camera mainCam
    {
        get
        {
            if (_cam == null)
            {
                _cam = Camera.main;
            }
            return _cam;
        }
    }

    static float _ratio = 0;
    public static float newRatio
    {
        get
        {
            if (_ratio == 0)
            {
                float oldRatio = 1080f / 1920f;
                _ratio = (float) Screen.width / Screen.height;
                _ratio /= oldRatio; //depend on width
            }
            return _ratio;
        }
    }

    private static Vector2 _size = Vector2.zero;
    public static Vector2 camSize
    {
        get
        {
            if (_size == Vector2.zero)
            {
                float orthographicSize = mainCam.orthographicSize;
                var x = mainCam.aspect * 2f * orthographicSize;
                var y = 2f * orthographicSize;
                _size = new(x, y);
            }

            return _size;
        }
    }
    
    public static bool IsInsideCamView(this Vector3 target, bool checkHor = true, bool checkVert = true)
    {
        var pos = mainCam.transform.position;
        var isInSide = true;
        
        if (checkHor)
        {
            var left = pos.x - camSize.x / 2f;
            var right = pos.x + camSize.x / 2f;
            isInSide = target.x.Between(left, right);
        }

        if (checkVert && isInSide)
        {
            var up = pos.y + camSize.y / 2f;
            var down = pos.y - camSize.y / 2f;
            isInSide = target.y.Between(down, up);
        }

        return isInSide;
    }

    #endregion

    #region Sort

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
    
    //shuffle không lặp lại list
    public static IList<T> ShuffleUnique<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T fruitCurrentIndex = list[i];
            int randomIndex = UnityEngine.Random.Range(
                Mathf.Min(i + 1, list.Count - 1), list.Count - 1);
            list[i] = list[randomIndex];
            list[randomIndex] = fruitCurrentIndex;
        }
        return list;
    }

    #endregion

    #region Random

    public static int Random(int num1, int num2, int seed = -1)
    {
        float val = Random((float) num1, num2, seed);
        int result = Mathf.RoundToInt(val);
        return result;
    }
    
    public static float Random(float num1, float num2, int seed = -1)
    {
        if (seed == -1) seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);
        return UnityEngine.Random.Range(num1, num2);
    }
    
    public static T2 LoadRandom<T, T2>(this Dictionary<T, T2> dic)
    {
        int rand = Random(0, dic.Count - 1);
        return dic.ElementAt(rand).Value;
    }
    
    public static T LoadRandom<T>(this IList<T> list, int seed = -1)
    {
        int rand = Random(0, list.Count - 1, seed);
        return list[rand];
    }
    
    public static T LoadRandom<T>(this IList<T> list, Func<T, bool> predic)
    {
        list = list.Where(predic).ToList();
        if (list.Count == 0)
        {
            return default(T);
        }
        int rand = UnityEngine.Random.Range(0, list.Count - 1);
        return list[rand];
    }
    
    public static IList<int> ToListIndex<T>(this IList<T> lst)
    {
        List<int> indexs = new List<int>();
        for (int i = 0; i < lst.Count; i++)
        {
            indexs.Add(i);
        }
        return indexs;
    }

    public static List<T> RandomFix<T>(this IList<T> lst, int take)
    {
        var result = lst.RandomUnique(take);
        for (int i = result.Count - 1; i < take; i++)
        {
            result.Add(lst.LoadRandom());
        }

        return result;
    }

    public static List<T> RandomUnique<T>(this IList<T> lst, int take)
    {
        var result = new List<T>();
        var tmp = new List<T>(lst);
        take = Mathf.Min(take, lst.Count);
        for (int i = 0; i < take; i++)
        {
            int rand = UnityEngine.Random.Range(0, tmp.Count - 1);
            result.Add(tmp[rand]);
            tmp.RemoveAt(rand);
        }

        return result;
    }
    
    public static List<T> RandomUnique<T>(this IList<T> lst, int take, Func<T, bool> predic)
    {
        lst = lst.Where(predic).ToList();
        if (lst.Count == 0)
        {
            return default(List<T>);
        }

        return RandomUnique(lst, take);
    }
    
    public static Vector3 GetRandomPointInsideCollider(this SphereCollider collider)
    {
        float extents = collider.radius / 2f;
        Vector3 point = new Vector3(
            Random(-extents, extents),
            Random(-extents, extents),
            Random(-extents, extents)
        ) + collider.center;
        point = collider.transform.TransformPoint(point);
        return point;
    }

    public static Vector3 GetRandomPointInsideCollider(this BoxCollider collider)
    {
        Vector3 extents = collider.size / 2f;
        Vector3 point = new Vector3(
            Random(-extents.x, extents.x),
            Random(-extents.y, extents.y),
            Random(-extents.z, extents.z)
        ) + collider.center;
        point = collider.transform.TransformPoint(point);
        return point;
    }

    public static Vector3 GetRandomPointInsideCollider(this CapsuleCollider collider)
    {
        float extentsX = collider.radius / 2f;
        float extentsY = collider.height / 2f;
        Vector3 point = new Vector3(
            Random(-extentsX, extentsX),
            Random(-extentsY, extentsY),
            Random(-extentsX, extentsX)
        ) + collider.center;
        point = collider.transform.TransformPoint(point);
        return point;
    }

    public static Vector3 GetRandomPointInsideCollider(this BoxCollider2D collider)
    {
        Vector2 extents = collider.size / 2f;
        Vector2 point = new Vector2(
            Random(-extents.x, extents.x),
            Random(-extents.y, extents.y)
        ) + collider.offset;
        point = collider.transform.TransformPoint(point);
        return point;
    }
    
    public static Vector3 GetRandomPointInsideCollider(this Collider2D collider)
    {
        var bounds = collider.bounds;
        
        var xMin = bounds.min.x;
        var xMax = bounds.max.x;
        var yMin = bounds.min.y;
        var yMax = bounds.max.y;
        
        Vector2 point = new Vector2(
            Random(xMin, xMax),
            Random(yMin, yMax)
        ) + collider.offset;
        point = collider.transform.TransformPoint(point);
        return point;
    }
    
    public static Vector2 GetContactPointNomalize(this Collider2D col, Transform thisTrans)
    {
        return col.GetContactPoint(thisTrans) - (Vector2)col.bounds.center;
    }
    
    public static Vector2 GetContactPointNomalize(this Collider2D col, Collider2D thisCol)
    {
        return col.GetContactPoint(
                   (Vector2) thisCol.transform.position + thisCol.offset)
               - (Vector2) col.bounds.center;
    }

    public static Vector2 GetContactPointNomalize(this Collision2D col)
    {
        return col.GetContact(0).normal;
    }

    public static Vector2 GetContactPoint(this Collider2D col, Transform thisTrans)
    {
        return col.bounds.ClosestPoint(thisTrans.position);
    }

    public static Vector2 GetContactPoint(this Collider2D col, Vector2 pos)
    {
        return col.bounds.ClosestPoint(pos);
    }

    public static Vector2 GetContactPoint(this Collision2D col)
    {
        return col.GetContact(0).point;
    }
    
    #endregion

    #region Dotween
    
    public static Tweener ChangeValueFloat(float startValue, float endValue, float speed,
        Action<float> onUpdate)
    {
        onUpdate?.Invoke(startValue);
        return DOTween.To(() => startValue, x => startValue = x, endValue, speed).OnUpdate(delegate
        {
            onUpdate?.Invoke(startValue);
        }).SetEase(Ease.Linear);
    }
    
    public static Tweener ChangeValueInt(int startValue, int endValue, float speed,
        Action<int> onUpdate)
    {
        onUpdate?.Invoke(startValue);
        return DOTween.To(() => startValue, x => startValue = x, endValue, speed).OnUpdate(delegate
        {
            onUpdate?.Invoke(startValue);
        }).SetEase(Ease.Linear);
    }
    
    public static async void DelayTime(float time, Action onDone)
    {
        await DOVirtual.DelayedCall(time, delegate
        {
            onDone();
        });
    }

    #endregion

    #region Convert
    
    public static Vector3 StringToVector3(this string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
            sVector = sVector.Substring(1, sVector.Length-2);
        }
 
        // split the items
        string[] sArray = sVector.Split(',');
 
        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
 
        return result;
    }
    
    public static Vector3Int StringToVector3Int(this string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
            sVector = sVector.Substring(1, sVector.Length-2);
        }
 
        // split the items
        string[] sArray = sVector.Split(',');
 
        // store as a Vector3
        Vector3Int result = new Vector3Int(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));
 
        return result;
    }

    public static List<T> ConvertToList<T>(this T item)
    {
        var result = new List<T>();
        result.Add(item);
        return result;
    }
    
    public static Vector2 RadianToVector2(this float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
      
    public static Vector2 DegreeToVector2(this float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }
    
    public static string ToJson(this object obj)
    {
        return JsonUtility.ToJson(obj);
    }
    
    public static T FromJson<T>(this string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
    
    private static readonly SortedDictionary<float, string> abbrevations = new SortedDictionary<float, string>
    {
        {1000, "k" },
        {1000000, "M" },
        {1000000000, "B" },
        {1000000000000, "T" }
    };

    public static string AbbreviateNumber(this int number)
    {
        return ((float)number).AbbreviateNumber();
    }

    public static string AbbreviateNumber(this float number)
    {
        for (int i = abbrevations.Count - 1; i >= 0; i--)
        {
            if (Mathf.Abs(number) >= 10000)
            {
                KeyValuePair<float, string> pair = abbrevations.ElementAt(i);
                if (Mathf.Abs(number) >= pair.Key)
                {
                    double roundedNumber = number / pair.Key;
                    //var result = Math.Round(roundedNumber, 2);
                    var result = string.Format("{0:0}", roundedNumber);
                    return result + pair.Value;
                }
            }
        }
        return string.Format("{0:0}", number);
    }

    public static string ToCurrency(this int number, string currency = "")
    {
        return number.ToString("N0") + currency;
    }

    public static List<Dropdown.OptionData> EnumToOptions<T>()
    {
        var options = new List<Dropdown.OptionData>();
        var names = Enum.GetNames(typeof(T));
        for (int i = 0; i < names.Length; i++)
        {
            options.Add(new Dropdown.OptionData(names[i]));
        }

        return options;
    }

    public static float ValidValue(this float value)
    {
        return (float)Math.Round(value, 2);
    }

    #endregion

    #region Number

    public static bool AreApproximatelyEqual(float a, float b, float tolerance = 0.0001f)
    {
        return Math.Abs(a - b) < tolerance;
    }
    
    private static readonly Dictionary<char, int> RomanNumberDictionary = new()
    {
            { 'I', 1 },
            { 'V', 5 },
            { 'X', 10 },
            { 'L', 50 },
            { 'C', 100 },
            { 'D', 500 },
            { 'M', 1000 },
    };
    private static readonly Dictionary<int, string> NumberRomanDictionary = new()
    {
        { 1000, "M" },
        { 900, "CM" },
        { 500, "D" },
        { 400, "CD" },
        { 100, "C" },
        { 90, "XC" },
        { 50, "L" },
        { 40, "XL" },
        { 10, "X" },
        { 9, "IX" },
        { 5, "V" },
        { 4, "IV" },
        { 1, "I" },
    };

    public static string NumberToRoman(this int number)
    {
        var roman = new StringBuilder();

        foreach (var item in NumberRomanDictionary)
        {
            while (number >= item.Key)
            {
                roman.Append(item.Value);
                number -= item.Key;
            }
        }

        return roman.ToString();
    }

    public static int RomanToNumber(this string roman)
    {
        int total = 0;

        int current, previous = 0;
        char currentRoman, previousRoman = '\0';

        for (int i = 0; i < roman.Length; i++)
        {
            currentRoman = roman[i];

            previous = previousRoman != '\0' ? RomanNumberDictionary[previousRoman] : '\0';
            current = RomanNumberDictionary[currentRoman];

            if (previous != 0 && current > previous)
            {
                total = total - (2 * previous) + current;
            }
            else
            {
                total += current;
            }

            previousRoman = currentRoman;
        }

        return total;
    }

    public static string FormatNumber(long value)
    {
        var str = value.ToString();
        if (value < Mathf.Pow(10, 3))
            str = value.ToString();
        else if (value >= Mathf.Pow(10, 9))
            str = (long)(value / Mathf.Pow(10, 9)) + "B";
        else if (value >= Mathf.Pow(10, 6))
            str = (long)(value / Mathf.Pow(10, 6)) + "M";
        else if (value >= Mathf.Pow(10, 3))
            str = (long)(value / Mathf.Pow(10, 3)) + "K";
        return str;
    }
    
    public static bool Between(this float val, float min, float max)
    {
        return val >= min && val <= max;
    }
    
    public static bool Between(this int val, int min, int max)
    {
        return val >= min && val <= max;
    }

    #endregion

    #region String
        
    public static string GetName(this string objName)
    {
        var name = objName.Split(" ")[0];
        name = name.Split("(")[0];
        return name;
    }
    
    public static bool IsEmail(this string input)
    {
        return !string.IsNullOrEmpty(input) && Regex.IsMatch(input,
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$");
    }
        
    public static int GetID(this string objName)
    {
        var name = objName.Split(" ")[0];
        name = name.Split("(")[0];
        if (name.Contains("_"))
            name = name.Split("_")[1];
        return int.Parse(name);
    }

    public static int StringToNumber(this string input)
    {
        string result = "";
        foreach (char c in input)
        {
            result += ((int)c).ToString();
        }

        int finalNumber = int.Parse(result);
        return finalNumber;
    }

    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        StringBuilder result = new StringBuilder();
        result.Append(char.ToLower(input[0]));
        for (int i = 1; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsUpper(c))
            {
                result.Append('_');
                result.Append(char.ToLower(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
    
    public static string UppercaseFirst(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    public static string RemoveSpace(this string input, bool upperCase = true)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = "";
        input.Split(" ").ToList().ForEach(
            delegate(string x)
            {
                var content = x.Replace(" ", "");
                if (upperCase) content = char.ToUpper(content[0]) + content.Substring(1, content.Length - 1);
                result += content;
            });
        return result;
    }
    
    public static string RandomString(int length, string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
    {
        if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
        if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

        const int byteSize = 0x100;
        var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
        if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

        // Guid.NewGuid and System.Random are not particularly random. By using a
        // cryptographically-secure random number generator, the caller is always
        // protected, regardless of use.
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            var result = new StringBuilder();
            var buf = new byte[128];
            while (result.Length < length)
            {
                rng.GetBytes(buf);
                for (var i = 0; i < buf.Length && result.Length < length; ++i)
                {
                    // Divide the byte into allowedCharSet-sized groups. If the
                    // random value falls into the last group and the last group is
                    // too small to choose from the entire allowedCharSet, ignore
                    // the value in order to avoid biasing the result.
                    var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                    if (outOfRangeStart <= buf[i]) continue;
                    result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                }
            }
            return result.ToString();
        }
    }
    
    public static string GetBetween(string strSource, string strStart, string strEnd)
    {
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            int start, end;
            start = strSource.IndexOf(strStart, 0) + strStart.Length;
            end = strSource.IndexOf(strEnd, start);
            return strSource.Substring(start, end - start);
        }
        return "";
    }

    public static string ShortString(this string strSource, int startCount = 6, int endCount = 5)
    {
        if (strSource.Length <= startCount + endCount) return strSource;
        return strSource.Substring(0, startCount) + "..." + strSource.Substring(strSource.Length - endCount);
    }

    #endregion

    #region Look

    // public static void LookAt2D(this Transform trans, Vector2 worldUp, Vector2 dir)
    // {
    //     trans.eulerAngles = Vector3.forward * Vector2.SignedAngle(worldUp, dir);
    // }

    public static void LookAt2D(this Transform trans, Vector3 target, float startAngle = 0)
    {
        Vector3 diff = target - trans.position;
        diff.Normalize();
 
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.Euler(0f, 0f, rot_z - 90 + startAngle);
    }

    public static Quaternion GetLookAtDir2D(this Vector3 trans, Vector3 target, float startAngle = 0)
    {
        Vector3 diff = target - trans;
        diff.Normalize();
 
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, rot_z - 90 + startAngle);
    }

    /// <summary>
    /// Make 3D gameobject x axis look at target in 2D (with object has default rotation like in 3D).
    /// </summary>
    /// <param name="trans">Trans.</param>
    /// <param name="targetTrans">Target trans.</param>
    public static void LookAtAxisX2D(this Transform trans, Transform targetTrans)
    {
        LookAtAxisX2D(trans, targetTrans.position);
    }
    /// <summary>
    /// Make 3D gameobject x axis look at target in 2D (with object has default rotation like in 3D).
    /// </summary>
    /// <param name="trans">Trans.</param>
    /// <param name="targetPosition">Target position.</param>
    public static void LookAtAxisX2D(this Transform trans, Vector3 targetPosition)
    {
        // It's important to know rotating direction (clock-wise or counter clock-wise)
        // If target is above of gameobject (has y value higher) then rotate counter clock-wise and vice versa
        bool isAboveOfXAxis = targetPosition.y > trans.position.y;
        float angle = (isAboveOfXAxis ? 1 : -1) * Vector3.Angle(Vector3.right, targetPosition - trans.position);
//        trans.localRotation = Quaternion.identity;
        trans.localRotation = Quaternion.Euler(Vector3.forward * angle);
    }
    /// <summary>
    /// Make 3D gameobject y axis look at target in 2D (with object has default rotation like in 3D).
    /// </summary>
    /// <param name="trans">Trans.</param>
    /// <param name="targetTrans">Target trans.</param>
    public static void LookAtAxisY2D(this Transform trans, Transform targetTrans)
    {
        LookAtAxisY2D(trans, targetTrans.position);
    }
    /// <summary>
    /// Make 3D gameobject y axis look at target in 2D (with object has default rotation like in 3D).
    /// </summary>
    /// <param name="trans">Trans.</param>
    /// <param name="targetPosition">Target position.</param>
    public static void LookAtAxisY2D(this Transform trans, Vector3 targetPosition)
    {
        var position = trans.position;
        bool isLeftOfYAxis = targetPosition.x < position.y;
        float angle = (isLeftOfYAxis ? 1 : -1) * Vector3.Angle(Vector3.up, targetPosition - position);
//        trans.localRotation = Quaternion.identity;
        trans.localRotation = Quaternion.Euler(Vector3.forward * angle);
    }
    /// 
    /// This is a 2D version of Quaternion.LookAt; it returns a quaternion
    /// that makes the local +X axis point in the given forward direction.
    /// 
    /// forward direction
    /// Quaternion that rotates +X to align with forward
    static void LookAt2D(this Transform transform, Vector2 forward)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }

    #endregion

    #region Time

    public static DateTime StartOfDay(this DateTime theDate)
    {
        return theDate.Date;
    }
    public static DateTime EndOfDay(this DateTime theDate)
    {
        return theDate.Date.AddDays(1).AddTicks(-1);
    }

    public static bool IsWeekend()
    {
        var currentDay = (int)System.DateTime.Now.DayOfWeek;
        return currentDay == 6 || currentDay == 0;
    }
    public static float TotalSecondsInADay()
    {
        return 86400;
    }
    
    public static DateTime ConvertTotalSecondsToDateTime(this double totalSeconds)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return origin.AddSeconds(totalSeconds);
    }
    public static DateTime ConvertTotalSecondsToDateTime(this long totalSeconds)
    {
        double convert = Convert.ToDouble(totalSeconds);
        return convert.ConvertTotalSecondsToDateTime();
    }
    
    private static readonly DateTime s_Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long ToUnixTimeSeconds(this DateTime date) =>
        Convert.ToInt64((date.ToUniversalTime() - s_Epoch).TotalSeconds);

    public static DateTime TimeSecondsToLocalDateTime(this long unixTimeInSeconds) =>
        s_Epoch.AddSeconds(unixTimeInSeconds).ToLocalTime();

    #endregion

    #region Enum

    public static Enum GetRandomEnumValue(this Type t)
    {
        return Enum.GetValues(t)          // get values from Type provided
            .OfType<Enum>()               // casts to Enum
            .OrderBy(e => Guid.NewGuid()) // mess with order of results
            .FirstOrDefault();            // take first item in result
    }

    public static void ForeachEnum(this Type t, Action<Enum> action)
    {
        var types = Enum.GetValues(t).OfType<Enum>();
        foreach (var type in types)
        {
            action(type);
        }
    }

    public static T ToEnum<T>(this string value)
    {
        if (string.IsNullOrEmpty(value)) return default;
        Enum.TryParse(typeof(T), value, true, out var result);
        return result is T resultEnum ? resultEnum : default;
    }

    #endregion

    #region Color

    public static string ToHexString(this Color c)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(c)}";
    }
    public static Color ToColor(this string hex)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    public static string SetColorText(this string text, string colorHex)
    {
        return $"<color=#{colorHex}>{text}</color>";
    }
    
    public static string SetColorText(this string text, Color color)
    {
        return text.SetColorText(color.ToHexString());
    }

    #endregion

    #region Transform & Object

    public static RectTransform GetRectTransform(this Transform trans)
    {
        return trans.GetComponent<RectTransform>();
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject)
        where T : Component
    {
        if (!gameObject.TryGetComponent<T>(out var component))
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }
    
    public static Type GetClass<T>(string className = "")
    {
        if (string.IsNullOrEmpty(className))
            className = typeof(T).Name;
        var classPath = className;
        if (!string.IsNullOrEmpty(typeof(T).Namespace))
            classPath = $"{typeof(T).Namespace}.{className}";
        
        return Type.GetType(classPath);
    }

    public static void DeleteAllChild<T>(this T target, int start = 0, int end = 0, bool destroyImmediate = false) where T : Transform
    {
        for (int i = target.childCount - 1 - end; i >= start; i--)
        {
            if (destroyImmediate)
                UnityEngine.Object.DestroyImmediate(target.GetChild(i).gameObject);
            else
                UnityEngine.Object.Destroy(target.GetChild(i).gameObject);
        }
    }

    public static void DeleteAllChildImmediate<T>(this T target, int start = 0, int end = 0) where T : Transform
    {
        for (int i = target.childCount - 1 - end; i >= start; i--)
        {
            UnityEngine.Object.DestroyImmediate(target.GetChild(i).gameObject);
        }
    }
    
    public static List<T> FillData<TD, T>(this Component component, IEnumerable<TD> data,
        Action<TD, T, int> itemAction = null)
        where T : Component
    {
        var res = new List<T>();
        var listData = data.ToList();
        var transform = component.transform;

        for (var i = 0; i < Mathf.Max(listData.Count, transform.childCount); i++)
        {
            if (i == transform.childCount) Object.Instantiate(transform.GetChild(0), transform);
            transform.GetChild(i).gameObject.SetActive(i < listData.Count);
            if (i < listData.Count)
            {
                var view = transform.GetChild(i).GetComponent<T>();
                if (view is IItemView<TD> tdView) tdView.Setup(listData[i]);
                res.Add(view);
                itemAction?.Invoke(listData[i], view, i);
            }
        }

        return res;
    }

    #endregion

    #region Store
    
    public static string StoreUrl()
    {
        return $"https://play.google.com/store/apps/details?id={Application.identifier}";
    }
    public static void GoToStore()
    {
        OpenUrl(StoreUrl());
    }

    #endregion

    #region Utils
    
    public static string DeviceID
    {
        get
        {
#if UNITY_EDITOR
            return SystemInfo.deviceUniqueIdentifier + Random(1, 10000);
#endif
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
    
    public static async UniTask<bool> CheckInternetConnection()
    {
        const string echoServer = "https://google.com";
        bool result;
        using (var request = UnityWebRequest.Head(echoServer))
        {
            request.timeout = 5;
            await request.SendWebRequest();
            result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
        }

        if (result)
            return true;
        return false;
    }

    public static void CopyToClipboard(this string textToCopy)
    {
        //UniClipboard.SetText(textToCopy);
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        var editor = new TextEditor {text = textToCopy};
        editor.SelectAll();
        editor.Copy();
#else
        GUIUtility.systemCopyBuffer = textToCopy;
#endif
    }
    
    public static T Clone<T>(this T source)
    {
        var serialized = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(serialized);
    }

    public static void CopyProperties(this object source, object destination)
    {
        if (source.GetType() != destination.GetType())
        {
            throw new ArgumentException("Source and destination types must match");
        }

        PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                property.SetValue(destination, property.GetValue(source, null), null);
            }
        }
    }

    public static void OpenUrl(this string url)
    {
        Debug.Log($">> open link {url}");
        Application.OpenURL(url);
    }
    
    public static float GetFps()
    {
        return 1f / Time.deltaTime;
    }
    
    private static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }
    
    public static string GetHashString(this string inputString)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($">> gen hash fail {e.Message}");
            return $">> gen hash fail {e.Message}";
        }
    }

    public static async void ShowNotiText(string content, Vector3 pos)
    {
        var objPref = await A.Get<GameObject>(MyKeys.Prefabs.NotiText);
        var obj = LeanPool.Spawn(objPref, PopupManager.transform);
        obj.transform.position = pos;
        obj.SendMessage(nameof(ISetData.SetData), content);
    }

    #endregion
}

public interface IItemView<T>
{
    void Setup(T data);
}

public static class ObjectMovementUtils
{
    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float progress)
    {
        if (progress > 1f)
        {
            progress = 1f; // Clamp t to 1 to prevent overshooting
        }

        // Calculate the current position along the parabola
        float u = 1 - progress;
        Vector2 position = u * start + progress * end + height * (4 * progress * u) * Vector2.up;
        return position;
    }

    public static Vector2 Wave(Vector2 startPoint, Vector2 endPoint, float amplitude, float frequency, float elapsedTime)
    {
        var totalTime = 1;
        float totalDistance = Vector2.Distance(startPoint, endPoint);
        float speed = totalDistance / totalTime; // Calculate speed based on total distance and time
        float t = Mathf.Clamp01(speed * elapsedTime / totalDistance);

        // Calculate the linear interpolation between start and end points
        Vector2 linearPosition = Vector2.Lerp(startPoint, endPoint, t);

        // Apply the wave effect
        float wave = amplitude * Mathf.Sin(t * frequency * Mathf.PI * 2);
        Vector2 wavePosition = new Vector2(0, wave);

        // Combine the linear and wave positions
        return linearPosition + wavePosition;
    }
}

public static class ScriptableObjectCreator
{
    public static T CreateAsset<T>(string folderPath, string fileName) where T : ScriptableObject
    {
#if UNITY_EDITOR
        // Đảm bảo folder tồn tại
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // Tạo instance
        T asset = ScriptableObject.CreateInstance<T>();

        // Đường dẫn lưu file
        string path = $"{folderPath}/{fileName}.asset";

        // Tạo asset và lưu
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select để dễ thấy
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
#else
        return null;
#endif
    }
}
