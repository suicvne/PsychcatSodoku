using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IgnoreSolutions.PsychSodoku;
using IgnoreSolutions.Sodoku;

public static class PsychSudokuSaveSerializer
{

    public static void DebugSave(string path)
    {
#if UNITY_IOS
        string fullText = File.ReadAllText(path);
        UnityEngine.Debug.Log(path);
        UnityEngine.Debug.Log(fullText);
#endif
    }

    public static PsychSudokuSave ReadSudokuSave(string path, LevelList levelList = null)
    {
        if(!File.Exists(path)) return null;

#if UNITY_IOS
        DebugSave(path);
#endif

        using (FileStream fs = File.OpenRead(path))
        {
            using(StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                string validation = sr.ReadLine();
                if(validation == "PsychSudokuSave")
                {
                    PsychSudokuSave newSave = PsychSudokuSave.Default(levelList);

                    string readLine = null;
                    while((readLine = sr.ReadLine()) != null
                        && readLine.Trim() != "END_SAVE")
                    {
                        if(readLine != null)
                        {
                            if (readLine.Contains("="))
                            {
                                ReadVariable(readLine, newSave);
                            }
                            else if (readLine.Trim() == "LevelSaveInformation")
                            {
                                LevelSaveInformation[] testReadLsi = ReadLevelSaveInformation(sr);
                                newSave._LevelSaveInformation = testReadLsi;
                                //UnityEngine.Debug.Log($"Level Save Info Count: {testReadLsi.Length}");
                            }
                            else if(readLine.Trim() == "SaveStateInformation")
                            {
                                SaveStateInformation testReadSaveState = ReadSaveStateInformation(sr);
                                newSave._SaveStateInformation = testReadSaveState;
                                UnityEngine.Debug.Log($"Level Has Valid Save State: {testReadSaveState._IsValidSaveState}");
                            }
                        }
                    }

                    return newSave;

                    // OK! Lets read
                }
            }
        }

        return null;
    }

    private static bool ReadVariable(string input, PsychSudokuSave save)
    {
        string[] inputSplit = input.Split(new char[] { '=' });

        //_LastCompletedLevel=15
        switch(inputSplit[0].Trim())
        {
            case "_LastCompletedLevel":
                save._LastCompletedLevel = int.Parse(inputSplit[1]);
                UnityEngine.Debug.Log($"_LastCompletedLevel: {save._LastCompletedLevel}");
                return true;
            default:
                UnityEngine.Debug.Log($"Found unknown variable in save: {inputSplit[0]} with value of {inputSplit[1]}");
                break;
        }

        return false;
    }

    private static Regex ExtractParamsNotInBrackets = new Regex("([\b,](?![^\\[]*\\]))", RegexOptions.Compiled);
    private static Regex ExtractParamsNotInSquiggly = new Regex("([\b,](?![^\\{]*\\}))", RegexOptions.Compiled);
    private static Regex SplitByColonForIndexing = new Regex("([\b:](?![^\\{]*\\}))", RegexOptions.Compiled);

    private static SaveStateInformation ReadSaveStateInformation(StreamReader sr)
    {
        SaveStateInformation ssi = new SaveStateInformation();

        string readLine = null;

        while((readLine = sr.ReadLine()) != null
            && readLine.Trim() != "END"
            && readLine.Trim() != "END\n")
        {
            if(readLine != null)
            {
                string[] paramList = ExtractParamsNotInBrackets.Split(readLine);
                int true_count = 0;
                for(int i = 0; i < paramList.Length; i++)
                {
                    if (paramList[i].Trim() == ",") continue;
                    switch(true_count)
                    {
                        // Is Valid Save State
                        case 0:
                            ssi._IsValidSaveState = bool.Parse(paramList[i]);
                            true_count++;
                            break;
                        // Last Level Index
                        case 1:
                            ssi._LastLevelIndex = int.Parse(paramList[i]);
                            true_count++;
                            break;
                        // Last Level Time
                        case 2:
                            ssi._LastLevelTime = double.Parse(paramList[i]);
                            true_count++;
                            break;
                        // Last Level Difficulty
                        case 3:
                            ssi._LastLevelDifficulty = (PlayDifficulty)Enum.Parse(typeof(PlayDifficulty), paramList[i]);
                            true_count++;
                            break;
                        // Player Modified Grid Spots
                        case 4:
                            ssi._PlayerModifiedGridSpots = ReadGridSpotInformationArray(paramList[i]);
                            //UnityEngine.Debug.Log($"Player Modified Grid Spots: {ssi._PlayerModifiedGridSpots.Length}");
                            true_count++;
                            break;
                    }
                }
            }
        }

        return ssi;
    }

    private static GridSpotInformation[] ReadGridSpotInformationArray(string input)
    {
        string withoutBrackets = input.Replace("[", "");
        withoutBrackets = withoutBrackets.Replace("]", "");
        string[] mainSplitList = ExtractParamsNotInSquiggly.Split(withoutBrackets);
        List<GridSpotInformation> gsiList = new List<GridSpotInformation>();

        int true_count = 0;
        for(int i = 0; i < mainSplitList.Length; i++)
        {
            string inputGsi = mainSplitList[i];
            if (inputGsi.Trim() == ",") continue;
            else
            {
                gsiList.Add(ParseGridSpotInformation(inputGsi));

                true_count++;
            }
        }

        return gsiList.ToArray();
    }

    private static GridSpotInformation ParseGridSpotInformation(string input)
    {
        // {0,0}
        GridSpotInformation newGridSpotInfo = new GridSpotInformation();

        string inputGsiNoSquiggly = input.Replace("{", "");
        inputGsiNoSquiggly = inputGsiNoSquiggly.Replace("}", "");

        string[] inputGsiParamList = inputGsiNoSquiggly.Split(',');
        for(int i = 0; i < inputGsiParamList.Length; i++)
        {
            switch(i)
            {
                // _IndexOnGrid
                case 0:
                    newGridSpotInfo._IndexOnGrid = int.Parse(inputGsiParamList[i].Trim());
                    break;
                // _FilledValue
                case 1:
                    newGridSpotInfo._FilledValue = int.Parse(inputGsiParamList[i].Trim());
                    break;
                case 2: // Possible Numbers
                    newGridSpotInfo._PossibleNumbers =
                        ParseIntArray(inputGsiParamList[i]);
                    break;
            }
        }

        return newGridSpotInfo;
    }

    private static int[] ParseIntArray(string input)
    {
        List<int> newIntArray = new List<int>();

        // [0,0,0,0] ?
        //

        return newIntArray.ToArray();
    }

    private static LevelSaveInformation[] ReadLevelSaveInformation(StreamReader sr)
    {
        // $"{index}: {lsi.LevelIndex}, {lsi._LevelCompletedOnce}, [bestTime], [bestScores]"

        List<LevelSaveInformation> lsi = new List<LevelSaveInformation>();

        string readLine = null;
        while((readLine = sr.ReadLine()) != null
            && readLine.Trim() != "END"
            && readLine.Trim() != "END\n")
        {
            if(readLine != null)
            {
                string[] initialSplit = SplitByColonForIndexing.Split(readLine);
                string index = initialSplit[0];
                string[] paramList = ExtractParamsNotInBrackets.Split(initialSplit[2]);

                LevelSaveInformation newLsi = new LevelSaveInformation();

                int true_count = 0;
                for (int i = 0; i < paramList.Length; i++)
                {
                    
                    if (paramList[i].Trim() == ",") continue;
                    switch(true_count)
                    {
                        // Level Index being referred to.
                        case 0:
                            newLsi.LevelIndex = int.Parse(paramList[i]);
                            true_count++;
                            break;

                        // Level Has Been Completed Once
                        case 1:
                            newLsi._LevelCompletedOnce = bool.Parse(paramList[i]);
                            true_count++;
                            break;

                        // Split for reading of best time
                        case 2:
                            //UnityEngine.Debug.Log("[PsychSaveSerializer] TODO: Best time");
                            newLsi._BestTimes = ParseAsDictionaryArray(paramList[i], (input) =>
                            {
                                return double.Parse(input);
                            });
                            true_count++;
                            break;

                        // Split for reading of best scores
                        case 3:
                            //UnityEngine.Debug.Log("[PsychSaveSerializer] TODO: Best scores");
                            newLsi._BestScores = ParseAsDictionaryArray(paramList[i], (input) =>
                            {
                                return int.Parse(input);
                            });
                            true_count++;
                            break;
                    }
                }

                lsi.Add(newLsi);
            }
        }

        return lsi.ToArray();
    }

    private static Dictionary<PlayDifficulty, T[]> ParseAsDictionaryArray<T>(string input,
        Func<string, T> parseFunc)
    {
        Dictionary<PlayDifficulty, T[]> newDictionary = new Dictionary<PlayDifficulty, T[]>();

        // [{EASY:0,0,0,0,0},{MEDIUM:0,0,0,0,0},{HARD:0,0,0,0,0}]
        string inputNoBrackets = input.Replace("]", "");
        inputNoBrackets = inputNoBrackets.Replace("[", "");

        string[] splitDictionaryValues = ExtractParamsNotInSquiggly.Split(inputNoBrackets);

        // {EASY:0,0,0,0,0}
        // {EASY:0,0,0,0,0}
        // {EASY:0,0,0,0,0}
        int true_count = 0;
        for(int i = 0; i < splitDictionaryValues.Length; i++)
        {
            string value = splitDictionaryValues[i];
            if (value.Trim() == ",") continue;
            else
            {
                string valueNoBrackets = value.Replace("}", "");
                valueNoBrackets = valueNoBrackets.Replace("{", "");
                //EASY: 0,0,0,0,0
                string[] splitDictChildArg = valueNoBrackets.Split(new char[] { ':' });

                newDictionary.Add((PlayDifficulty)Enum.Parse(typeof(PlayDifficulty), splitDictChildArg[0].Trim()),
                    ParseAsTArray<T>(splitDictChildArg[1], parseFunc));


                true_count++;
            }
        }

        return newDictionary;
    }

    private static T[] ParseAsTArray<T>(string input, Func<string, T> parseFunc)
    {
        // 0,0,0,0,0

        string[] inputSplit = input.Split(new char[] { ',' });
        T[] returnVal = new T[inputSplit.Length];

        for(int i = 0; i < returnVal.Length; i++)
        {
            returnVal[i] = parseFunc.Invoke(inputSplit[i]);
        }

        return returnVal;
    }

    #region Write
    public static void WriteSudokuSave(this PsychSudokuSave save, string path)
    {
        if (File.Exists(path)) File.WriteAllText(path, "");
        using(FileStream fs = File.OpenWrite(path))
        {
            
            using(StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine($"PsychSudokuSave");
                WriteLevelSaveInformation(sw, save);
                sw.WriteLine($"END_SAVE");
            }
        }
    }

    private static void WriteLevelSaveInformation(StreamWriter sw,
        PsychSudokuSave save)
    {
        sw.WriteLine($" LevelSaveInformation");
        for(int i = 0; i < save._LevelSaveInformation.Length; i++)
        {
            LevelSaveInformation lsi = save._LevelSaveInformation[i];

            WriteIndividualSaveInfo(sw, lsi, i);
        }
        sw.WriteLine($" END");

        sw.WriteLine($" SaveStateInformation");
        WriteSaveStateInformation(sw, save._SaveStateInformation);
        sw.WriteLine($" END");

        sw.WriteLine($" _LastCompletedLevel={save._LastCompletedLevel}");
    }

    private static void WriteSaveStateInformation(StreamWriter sw,
        SaveStateInformation ssi)
    {
        StringBuilder sb = new StringBuilder();

        // Initial values
        sb.Append($"  {ssi._IsValidSaveState}, {ssi._LastLevelIndex}, {ssi._LastLevelTime}, {ssi._LastLevelDifficulty}");

        // Write grid spot

        if(ssi._PlayerModifiedGridSpots != null
            && ssi._PlayerModifiedGridSpots.Length > 0)
        {
            sb.Append($", [");
            // Write each grid spot info
            for(int x = 0; x < ssi._PlayerModifiedGridSpots.Length; x++)
            {
                GridSpotInformation gsi = ssi._PlayerModifiedGridSpots[x];
                sb.Append($"{{{gsi._IndexOnGrid},{gsi._FilledValue}");

                if (ssi._PlayerModifiedGridSpots[x]._PossibleNumbers != null
                    && ssi._PlayerModifiedGridSpots[x]._PossibleNumbers.Length > 0)
                {
                    sb.Append(",[");
                    // Write each possible number for a given grid spot
                    for (int i = 0; i < ssi._PlayerModifiedGridSpots[x]._PossibleNumbers.Length; i++)
                    {
                        sb.Append($"{ssi._PlayerModifiedGridSpots[x]._PossibleNumbers[i]}{(i == ssi._PlayerModifiedGridSpots[x]._PossibleNumbers.Length - 1 ? "" : ",")}");
                    }
                    sb.Append("]");
                }

                sb.Append($"}}{(x == ssi._PlayerModifiedGridSpots.Length - 1 ? "" : ",")}");
            }
            sb.Append($"]");
        }

        sw.WriteLine(sb.ToString());
    }

    private static void WriteIndividualSaveInfo(StreamWriter sw,
        LevelSaveInformation lsi,
        int index)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"  {index}: {lsi.LevelIndex}, {lsi._LevelCompletedOnce}");

        if (lsi._BestTimes != null
            && lsi._BestTimes.Count > 0)
        {
            sb.Append(", [");
            for (int i = 0; i < lsi._BestTimes.Count; i++)
            {
                var bestTimeKeys = lsi._BestTimes.Keys.ToArray();
                var bestTimeValues = lsi._BestTimes.Values.ToArray();

                sb.Append("{");
                // Write Difficulty
                sb.Append($"{bestTimeKeys[i]}:");

                // Write Corresponding Array
                for (int x = 0; x < bestTimeValues[i].Length; x++)
                {
                    sb.Append($"{bestTimeValues[i][x]}{(x == bestTimeValues[i].Length - 1 ? "" : ",")}");
                }
                sb.Append($"}}{(i == lsi._BestTimes.Count - 1 ? "" : ",")}");
            }
            sb.Append("]");
        }

        if(lsi._BestScores != null
            && lsi._BestScores.Count > 0)
        {
            sb.Append($", [");
            for(int i = 0; i < lsi._BestScores.Count; i++)
            {
                var bestScoreKeys = lsi._BestScores.Keys.ToArray();
                var bestScoreValues = lsi._BestScores.Values.ToArray();

                sb.Append("{");
                sb.Append($"{bestScoreKeys[i]}:");
                for(int x = 0; x < bestScoreValues[i].Length; x++)
                {
                    sb.Append($"{bestScoreValues[i][x]}{(x == bestScoreValues[i].Length - 1 ? "" : ",")}");
                }

                sb.Append($"}}{(i == lsi._BestScores.Count - 1 ? "" : ",")}");
            }
            sb.Append("]");
        }

        sw.WriteLine(sb.ToString());
    }
    #endregion
}
