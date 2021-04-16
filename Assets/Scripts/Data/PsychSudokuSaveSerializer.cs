using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IgnoreSolutions.PsychSodoku;

public static class PsychSudokuSaveSerializer
{
    public static PsychSudokuSave ReadSudokuSave(string path)
    {

        using (FileStream fs = File.OpenRead(path))
        {
            using(StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                string validation = sr.ReadLine();
                if(validation == "PsychSudokuSave")
                {
                    string readLine = null;
                    while((readLine = sr.ReadLine()) != null
                        && readLine.Trim() != "END_SAVE")
                    {
                        if(readLine != null)
                        {
                            if (readLine.Contains("="))
                            {
                                // TODO: Variable read handling
                                UnityEngine.Debug.Log($"TODO: variable handling for line '{readLine}'");
                            }
                            else if (readLine.Trim() == "LevelSaveInformation")
                            {
                                LevelSaveInformation[] testReadLsi = ReadLevelSaveInformation(sr);
                                UnityEngine.Debug.Log($"Level Save Info Count: {testReadLsi.Length}");
                            }
                            else if(readLine.Trim() == "SaveStateInformation")
                            {
                                SaveStateInformation testReadSaveState;

                            }
                        }
                    }
                    // OK! Lets read
                }
            }
        }

        return null;
    }

    private static Regex ExtractParamsNotInBrackets = new Regex("([\b,](?![^\\[]*\\]))", RegexOptions.Compiled);

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
                string[] initialSplit = readLine.Split(new char[] { ':' });
                string index = initialSplit[0];
                string[] paramList = ExtractParamsNotInBrackets.Split(initialSplit[1]);

                LevelSaveInformation newLsi = new LevelSaveInformation();

                int true_count = 0;
                for (int i = 0; i < paramList.Length; i++)
                {
                    
                    if (paramList[i].Trim() == ",") continue;
                    switch(true_count)
                    {
                        // Level Index being referred to.
                        case 0: newLsi.LevelIndex = int.Parse(paramList[i]); true_count++; break;

                        // Level Has Been Completed Once
                        case 1: newLsi._LevelCompletedOnce = bool.Parse(paramList[i]); true_count++; break;

                        // Split for reading of best time
                        case 2: UnityEngine.Debug.Log("[PsychSaveSerializer] TODO: Best time"); break;

                        // Split for reading of best scores
                        case 3: UnityEngine.Debug.Log("[PsychSaveSerializer] TODO: Best scores"); break;
                    }
                }

                lsi.Add(newLsi);
            }
        }

        return lsi.ToArray();
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

                sb.Append("}");
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

                sb.Append($"{bestTimeKeys[i]}:");
                for (int x = 0; x < bestTimeValues[i].Length; x++)
                {
                    sb.Append($"{bestTimeValues[i][x]}{(x == bestTimeValues[i].Length - 1 ? "" : ",")}");
                }

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

                sb.Append($"{bestScoreKeys[i]}:");
                for(int x = 0; x < bestScoreValues[i].Length; x++)
                {
                    sb.Append($"{bestScoreValues[i][x]}{(x == bestScoreValues[i].Length - 1 ? "" : ",")}");
                }
            }
            sb.Append("]");
        }

        sw.WriteLine(sb.ToString());
    }
    #endregion
}
