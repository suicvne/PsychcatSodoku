using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IgnoreSolutions.Sodoku;
using Newtonsoft.Json;
using UnityEngine;
using static ModifyShaderOffset;

namespace IgnoreSolutions.PsychSodoku
{
    [Serializable]
    public struct LevelSaveInformation
    {
        public int LevelIndex;
        public bool _LevelCompletedOnce;

        public Dictionary<PlayDifficulty, double[]> _BestTimes;
        //public long[] _BestTimes;

        public Dictionary<PlayDifficulty, int[]> _BestScores;
        //public int[] _BestScores;


        public static LevelSaveInformation Default()
        {
            LevelSaveInformation lsi = new LevelSaveInformation
            {
                LevelIndex = -1,
                _LevelCompletedOnce = false
            };

            lsi._BestTimes = new Dictionary<PlayDifficulty, double[]>();
            lsi._BestTimes.Add(PlayDifficulty.EASY, new double[5]);
            lsi._BestTimes.Add(PlayDifficulty.MEDIUM, new double[5]);
            lsi._BestTimes.Add(PlayDifficulty.HARD, new double[5]);

            lsi._BestScores = new Dictionary<PlayDifficulty, int[]>();

            lsi._BestScores.Add(PlayDifficulty.EASY, new int[5]);
            lsi._BestScores.Add(PlayDifficulty.MEDIUM, new int[5]);
            lsi._BestScores.Add(PlayDifficulty.HARD, new int[5]);

            return lsi;
        }
    }

    [Serializable]
    public struct SaveStateInformation
    {
        public bool _IsValidSaveState;
        public int _LastLevelIndex;
        public double _LastLevelTime;
        public PlayDifficulty _LastLevelDifficulty;

        public GridSpotInformation[] _PlayerModifiedGridSpots;
        // TODO: Add list of filled cells with index,
        //       store level "notes" also.

        public static SaveStateInformation Default()
        {
            return new SaveStateInformation
            {
                _IsValidSaveState = false,
                _LastLevelIndex = -1,
                _LastLevelTime = -1,
                _PlayerModifiedGridSpots = new GridSpotInformation[49]
            };
        }
    }

    [Serializable]
    public struct GridSpotInformation
    {
        public int _IndexOnGrid;
        public int _FilledValue;
        public int[] _PossibleNumbers;

        public static GridSpotInformation Default()
        {
            return new GridSpotInformation
            {
                _IndexOnGrid = -1,
                _FilledValue = -1,
                _PossibleNumbers = new int[9]
            };
        }
    }

    public class PsychSudokuSave : ScriptableObject
    {
        //[SerializeField] LevelList _LevelList;

        /// <summary>
        /// The full list of levels in the game
        /// and whether or not they've been completed.
        ///
        /// Also stores and associates best times & best scores.
        /// </summary>
        [SerializeField] public LevelSaveInformation[] _LevelSaveInformation;

        [SerializeField] public SaveStateInformation _SaveStateInformation;

        /// <summary>
        /// The last level index that was FULLY completed (at least one difficulty complete).
        /// </summary>
        [SerializeField] public int _LastCompletedLevel = -1;

        private static string ApplicationSavePath
        {
            get
            {
                if (Application.isMobilePlatform || Application.isConsolePlatform)
                    return Application.persistentDataPath;
                else
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PsychCatSudoku");
            }
        }

        internal static string SaveFile
        {
            get
            {
                return Path.Combine(ApplicationSavePath, "state.json");
            }
        }

        public void SetLevelIndexCompleted(PlayDifficulty difficulty, int levelIndex, double levelCompleteTime)
        {
            // Update our last completed level index.
            _LastCompletedLevel = levelIndex;

            // Init stack with our existing times
            List<double> bestTimes = new List<double>(_LevelSaveInformation[levelIndex]._BestTimes[difficulty]);
            // Push our new level complete time
            bestTimes.Add(levelCompleteTime);

            // Order by lowest times first.
            // TODO: Does this happen?
            // Do I need to init some fake times to try and beat?
            bestTimes.OrderBy(x => x);

            // Best Times
            _LevelSaveInformation[levelIndex]._BestTimes[difficulty] = bestTimes.ToArray();

            bool success = PsychSudokuSave.WriteSaveJSON(this);

            if (success) Debug.Log($"Saved progress.");
            else Debug.LogError($"Unable to save progress. [PsychSudokuSave] Set Level Index Completed");
        }

        public static PsychSudokuSave Default(LevelList _levelList = null)
        {
            PsychSudokuSave pss = CreateInstance<PsychSudokuSave>();

            // Init level list for save.
            if(_levelList != null)
            {
                pss._LevelSaveInformation =
                    new LevelSaveInformation[_levelList.GetLevelList().Count];

                for(int i = 0; i < _levelList.GetLevelList().Count; i++)
                {
                    LevelData level = _levelList.GetLevelList()[i];

                    pss._LevelSaveInformation[i] = LevelSaveInformation.Default();
                    pss._LevelSaveInformation[i].LevelIndex = i;
                }
            }

            pss._SaveStateInformation = SaveStateInformation.Default();


            return pss;
        }

        public static bool WriteSaveJSON(PsychSudokuSave save)
        {
            try
            {
                File.WriteAllText(SaveFile, JsonConvert.SerializeObject(save));
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError($"Exception while trying to write save to `{SaveFile}`\n{ex.Message}\n\n{ex.StackTrace}");
            }


            return false;
        }

        public static PsychSudokuSave ReadSaveFromJSON()
        {
            try
            {
                if (File.Exists(SaveFile))
                {
                    string jText = File.ReadAllText(SaveFile);
                    return JsonConvert.DeserializeObject<PsychSudokuSave>(jText);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Exception while trying to write save to `{SaveFile}`\n{ex.Message}\n\n{ex.StackTrace}");
            }

            return null;
        }

        private static int GetLevelIndexInList(LevelList list, LevelData data)
        {
            for(int i = 0; i < list.GetLevelList().Count; i++)
            {
                if (list.GetLevelList()[i] == data) return i;
            }

            return -1;
        }

        public static SaveStateInformation SetSaveState(PsychSudokuSave save,
            ModifyShaderOffset gameState,
            LevelList levelList,
            GameTimeManager gameTime)
        {
            SaveStateInformation ssi = new SaveStateInformation();

            // Occupy general game level info into SaveStateInformation
            Tuple<PlayDifficulty, LevelData> gameLevelInfo = gameState.GetLevelInformation();

            ssi._LastLevelDifficulty = gameLevelInfo.Item1;

            ssi._LastLevelIndex = GetLevelIndexInList(levelList, gameLevelInfo.Item2);
            if(ssi._LastLevelIndex == -1)
            {
                throw new Exception($"Given level data, {gameLevelInfo.Item2}, was not found on the level list {levelList}");
            }

            if (gameTime != null)
            {
                ssi._LastLevelTime = gameTime.GetPlayTime().TotalSeconds;
            }

            // Get a list of all modified grid spots and then
            // convert that information to a serializable format.
            List<SodukoGriidSpot> gridSpots = gameState.GetGridSpots();
            List<SodukoGriidSpot> modifiedGridSpots = new List<SodukoGriidSpot>();
            
            foreach(var gridSpot in gridSpots)
            {
                if (gridSpot._SquareFilledValue != -1)
                {
                    modifiedGridSpots.Add(gridSpot);
                    continue;
                }

                if(gridSpot.PossibleNumbers.Count > 0)
                {
                    modifiedGridSpots.Add(gridSpot);
                }
            }

            ssi._PlayerModifiedGridSpots = new GridSpotInformation[modifiedGridSpots.Count];
            for(int i = 0; i < modifiedGridSpots.Count; i++)
            {
                var modGridSpot = modifiedGridSpots[i];

                ssi._PlayerModifiedGridSpots[i]._IndexOnGrid = modGridSpot._LevelIndex;
                ssi._PlayerModifiedGridSpots[i]._FilledValue = modGridSpot._SquareFilledValue;
                ssi._PlayerModifiedGridSpots[i]._PossibleNumbers = modGridSpot.PossibleNumbers.ToArray();
            }

            return ssi;
        }

        public static LevelData GetNextUnfinishedLevel(PsychSudokuSave save, LevelList _levelList)
        {
            var levelList = _levelList.GetLevelList();

            if(save._LastCompletedLevel != -1)
            {
                int index = save._LastCompletedLevel + 1;
                if (index >= levelList.Count) return null;

                return levelList[index];
            }
            

            // You completed the game!
            return null;
        }
    }
}