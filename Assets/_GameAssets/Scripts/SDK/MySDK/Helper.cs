using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Helper
{
    private static readonly System.Random _rand = new();

    public static string LoadTextFileFromResource(string path)
    {
        string filePath = path.Replace(".json", "");
        TextAsset file = Resources.Load<TextAsset>(filePath);
        return file.text;
    }

    public static async Task<string> LoadTextAssetFromResourceAsync(string filePath)
    {
        ResourceRequest request = Resources.LoadAsync(filePath);
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

        request.completed += operation =>
        {
            TextAsset textAsset = request.asset as TextAsset;
            tcs.SetResult(textAsset.text);
        };

        return await tcs.Task;
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

    public static T GetRandomElement<T>(T[] array)
    {
        int index = _rand.Next(0, array.Length);
        return array[index];
    }

    public static T GetRandomElement<T>(List<T> array)
    {
        int index = _rand.Next(0, array.Count);
        return array[index];
    }
}
