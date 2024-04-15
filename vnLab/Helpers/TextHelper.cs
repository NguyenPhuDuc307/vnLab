using System.Text;
using System.Text.RegularExpressions;

namespace vnLab.Helpers;

public class TextHelper
{
    public static string ToUnsignedString(string input)
    {
        input = input.Trim();
        for (int i = 0x20; i < 0x30; i++)
        {
            input = input.Replace(((char)i).ToString(), " ");
        }
        input = input.Replace(".", "-");
        input = input.Replace(" ", "-");
        input = input.Replace(",", "-");
        input = input.Replace(";", "-");
        input = input.Replace(":", "-");
        input = input.Replace("  ", "-");
        Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
        string str = input.Normalize(NormalizationForm.FormD);
        string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
        while (str2.IndexOf("?") >= 0)
        {
            str2 = str2.Remove(str2.IndexOf("?"), 1);
        }
        while (str2.Contains("--"))
        {
            str2 = str2.Replace("--", "-").ToLower();
        }
        return str2.ToLower();
    }

    public static string ToHexString(string str)
    {
        var sb = new StringBuilder();

        var bytes = Encoding.Unicode.GetBytes(str);
        foreach (var t in bytes)
        {
            sb.Append(t.ToString("X2"));
        }

        return sb.ToString();
    }

    public static string FromHexString(string hexString)
    {
        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }

        return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
    }

    public static string[] Split(string? str, string chr)
    {
        if (str == null)
            return new string[0];
        return str.Split(chr);
    }

    public static string Join(string keyword, string[]? strings)
    {
        return strings!.Length > 0 ? string.Join(keyword, strings) : "";
    }



    public static int[] AddArray(int[] arr, int value)
    {
        int[] newArr = new int[arr.Count() + 1];
        for (int i = 0; i < arr.Count() + 1; i++)
        {
            if (i < arr.Count())
                newArr[i] = arr[i];
            else
                newArr[i] = value;
        }
        return newArr;
    }
}