﻿using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Victoria.Player;

namespace Sketch_Bot.Models
{
    public static class HelperFunctions
    {
        public static string StripUnicodeCharactersFromString(string inputValue)
        {
            return Regex.Replace(inputValue, @"[^\u0000-\u007F]", string.Empty);
        }
        public static string StripUnicodeCharactersFromString(string inputValue, string replaceValue)
        {
            return Regex.Replace(inputValue, @"[^\u0000-\u007F]", replaceValue);
        }
        public static string StripUnicodeCharactersFromStringWithMatches(string inputValue)
        {
            int matches = Regex.Matches(inputValue, @"[^\u0000-\u007F]").Count;
            string replaced = Regex.Replace(inputValue, @"[^\u0000-\u007F]", string.Empty);
            return $"{replaced}-{matches}";
        }
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if (result == "")
            {
                if (process.ExitCode == 127)
                {
                    return $"Command `{cmd}` not found";
                }
                else if (process.ExitCode == 0)
                {
                    return "Command executed successfully";
                }
                else
                {
                    return $"{process.StandardError.ReadToEnd()}";
                }
            }
            return result;

        }
        public static List<string> AddToStartAndEnding(List<string> theList, string toStart, string toEnding)
        {
            List<string> newList = new List<string>();
            foreach (var item in theList)
            {
                newList.Add(toStart + item + toEnding);
            }
            return newList;
        }
        public static IEnumerable<string> AddToStartAndEnding(IEnumerable<string> theList, string toStart, string toEnding)
        {
            List<string> newList = new List<string>();
            foreach (var item in theList)
            {
                newList.Add(toStart + item + toEnding);
            }
            return newList as IEnumerable<string>;
        }
        public static List<string> MergeListStrings(List<string> list1, List<string> list2)
        {
            var newList = new List<string>();
            for (int i = 0; i < list1.Count; i++)
            {
                newList.Add(list1[i] + list2[i]);
            }
            return newList;
        }
        public static IEnumerable<string> MergeListStrings(IEnumerable<string> list1, IEnumerable<string> list2)
        {
            var newList = new List<string>();
            for (int i = 0; i < list1.ToList().Count; i++)
            {
                newList.Add(list1.ToList()[i] + list2.ToList()[i]);
            }
            return newList as IEnumerable<string>;
        }
        public static string CutString(string inputString, int after)
        {
            if (inputString.Length > after)
            {
                return inputString.Substring(0, after);
            }
            return inputString;
        }
        public static string ConvertSecondsToDate(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);

            if (t.Days > 0)
                return t.ToString(@"d\d\,\ hh\:mm\:ss");
            return t.ToString(@"hh\:mm\:ss");
        }
        public static string ConvertMillisecondsToDate(double milliseconds)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);

            if (t.Days > 0)
                return t.ToString(@"d\d\,\ hh\:mm\:ss");
            return t.ToString(@"hh\:mm\:ss");
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static bool ShouldPlayNext(this TrackEndReason trackEndReason)
        {
            return trackEndReason == TrackEndReason.Finished || trackEndReason == TrackEndReason.LoadFailed;
        }

        /// <summary>
        /// Returns a shortened form a given Emote. e.g. ":long_emote_name:" to ":l:". 
        /// If the string is an Emoji, e.g. :grimasse: it returns it as is.
        /// Otherwise raises ArgumentException if invalid string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The shortened form of the emote.</returns>
        public static string ToShortEmote(this string s)
        {
            if (!(Emote.TryParse(s, out _) || Emoji.TryParse(s, out _)))
                throw new ArgumentException($"{s} not a valid Emote sequence");

            var emote = Regex.Match(s, @"<(a?):(.+):(\d{18})>");
            if (!emote.Success)
            {
                return s;
            }
            return $"<{emote.Groups[1].Value}:{emote.Groups[2].Value[0]}:{emote.Groups[3].Value}>";
        }

        /// <summary>
        ///     A calculation along with its result.
        /// </summary>
        public struct Calculation
        {
            /// <summary>
            ///     The base equation executed over the converter.
            /// </summary>
            public string Equation { get; set; }

            /// <summary>
            ///     The equation result. NaN or default(double) if unsuccesful.
            /// </summary>
            public double Result { get; set; } = double.NaN;

            /// <summary>
            ///     The error of this calculation. If no error occurred, this will be <see cref="string.Empty"/>
            /// </summary>
            public string Error { get; set; } = string.Empty;

            public Calculation(string equation, string errorReason = "")
            {

                Equation = ConvertPower(equation);
                Console.WriteLine(Equation);

                if (!string.IsNullOrEmpty(errorReason))
                    Error = errorReason;

                try
                {
                    var result = new System.Data.DataTable()
                        .Compute(Equation, string.Empty);

                    if (result == DBNull.Value)
                        Error = "The result of this calculation is not a number.";

                    else
                        Result = Convert.ToDouble(result);
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
                string ConvertPower(string input)
                {
                    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    var foundIndexes = new List<int>();

                    for (int i = 0; i < input.Length; i++) if (input[i] == '^') foundIndexes.Add(i);

                    for (int j = 0; j < foundIndexes.Count; j++)
                    {
                        var index = input.IndexOf('^');
                        var left = input.Substring(0, index);
                        var right = input.Substring(index + 1, input.Length - index - 1);

                        var leftNumber = "";
                        var rightNumber = "";

                        for (int i = left.Length - 1; i >= 0; i--)
                        {
                            if (char.IsDigit(left[i]) || left[i] == '.')
                            {
                                leftNumber = left[i] + leftNumber;
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (int i = 0; i < right.Length; i++)
                        {
                            if (char.IsDigit(right[i]) || right[i] == '.')
                            {
                                rightNumber += right[i];
                            }
                            else
                            {
                                break;
                            }
                        }

                        var leftIndex = left.Length - leftNumber.Length;
                        var rightIndex = rightNumber.Length;

                        var leftPart = left.Substring(0, leftIndex);
                        var rightPart = right.Substring(rightIndex, right.Length - rightIndex);

                        var newString = leftPart + Math.Pow(double.Parse(leftNumber), double.Parse(rightNumber)) + rightPart;

                        input = newString;
                    }
                    return input;
                }
            }


            /// <summary>
            ///     Returns a string of the equation result.
            /// </summary>
            /// <returns>
            ///     The equation result.
            /// </returns>
            public override string ToString()
                => $"{Result}";
        }
    }
}
