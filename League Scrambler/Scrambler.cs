using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using League;
using League.Tools;
using League.Utils;
using League.Files;

namespace LeagueScrambler
{
    public class Scrambler
    {
        public string LeaguePath { get; set; }

        private SkinGroup[] _skinTable;
        private SkinGroup[] _skinTableScrambled;
        private List<ScramblerChangeArray> _changes;
        private Dictionary<string, string> _patchList;
        private Dictionary<string, ArchiveFile> _files;

        public Scrambler(string leaguepath)
        {
            LeaguePath = leaguepath;
            Initialize();
        }

        private void Initialize()
        {
            SkinGroupReader reader = new SkinGroupReader();
            _skinTable = reader.Read(Properties.Resources.SkinGroups);

            _changes = new List<ScramblerChangeArray>();
            PathListReader pr = new PathListReader();
            _changes.Add(new ScramblerChangeArray(pr.Read(Properties.Resources.LoadScreenPaths)));
            _changes.Add(new ScramblerChangeArray(pr.Read(Properties.Resources.AbilityIconPaths)));
            _changes.Add(new ScramblerChangeArray(pr.Read(Properties.Resources.CircleIconPaths)));
            _changes.Add(new ScramblerChangeArray(pr.Read(Properties.Resources.SquareIconPaths)));
            _changes.Add(new ScramblerChangeArray(pr.Read(Properties.Resources.ItemIconPaths)));
        }

        public void Scramble(Settings settings, int seed)
        {
            Console.WriteLine(new string('#', 50));
            Console.WriteLine("Scrambling files with seed {0}", seed);
            Console.WriteLine(new string('#', 50));

            if (seed == 1337) // League of Draven
            {
                var skindraven = _skinTable[0];
                for (int i = 0; i < _skinTable.Length; i++)
                {
                    for (int j = 0; j < _skinTable[i].Skins.Count; j++)
                    {
                        if (_skinTable[i].Skins[j].BlndFile.Contains("Draven/Skins/Base/Draven_Base.blnd"))
                        {
                            skindraven = _skinTable[i];
                            i = _skinTable.Length;
                            break;
                        }
                    }
                }
                _skinTableScrambled = new SkinGroup[_skinTable.Length];
                for (int i = 0; i < _skinTable.Length; i++)
                {
                    _skinTableScrambled[i] = skindraven;
                }

                settings[0] = true;

                var draven = "";
                for (int i = 0; i < _changes.Count; i++)
                {
                    settings[i + 1] = true;
                    for (int j = 0; j < _changes[i].Original.Length; j++)
                    {
                        if (_changes[i].Original[j].ToLower().Contains("draven"))
                        {
                            draven = _changes[i].Original[j];
                            break;
                        }
                    }
                    _changes[i].Scrambled = new string[_changes[i].Original.Length];
                    for(int j = 0; j < _changes[i].Scrambled.Length; j++)
                    {
                        _changes[i].Scrambled[j] = draven;
                    }
                }
            }
            else // Regular
            {
                var random = new Random(seed);
                for (int i = 0; i < _changes.Count; i++)
                {
                    _changes[i].Scrambled = ScrambleArray(_changes[i].Original, random);
                }
                _skinTableScrambled = ScrambleArray(_skinTable, random);
            }

            _patchList = new Dictionary<string, string>();

            CombineChanges(settings);
        }

        private void CombineChanges(Settings settings)
        {
            // Add skins to the change list
            if (settings[0])
            {
                for (int i = 0; i < _skinTable.Length; i++)
                {
                    for (int j = 0, k = 0; j < _skinTable[i].Skins.Count; j++)
                    {
                        AddSkinChange(_skinTable[i].Skins[j], _skinTableScrambled[i].Skins[k]);

                        // Loop the target sking group indexer around in case the new skin group has less skins than the original
                        k++;
                        if (k == _skinTableScrambled[i].Skins.Count)
                            k = 0;
                    }
                }
            }

            for(int i = 0; i < _changes.Count; i++)
            {
                if (settings[i + 1])
                {
                    for (int j = 0; j < _changes[i].Original.Length; j++)
                    {
                        AddChangeToList(_changes[i].Original[j], _changes[i].Scrambled[j]);
                    }
                }
            }
        }

        private T[] ScrambleArray<T>(T[] input, Random random)
        {
            // Generate an index table we can remove stuff out of
            List<int> freeIndexes = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                freeIndexes.Add(i);
            }

            // Fill and scramble the second table
            var result = new T[input.Length];
            for (int i = 0; i < result.Length; i++)
            {
                int index = random.Next(0, freeIndexes.Count);
                result[i] = input[freeIndexes[index]];
                freeIndexes.RemoveAt(index);
            }
            return result;
        }

        public void Patch(ArchiveFileManager manager)
        {
            _files = new Dictionary<string, ArchiveFile>();
            var fileList = _patchList.Values.ToArray();

            manager.BeginWriting();

            for (int i = 0; i < fileList.Length; i++)
            {
                Console.Title = string.Format("Fetching files... {0} / {1}", i + 1, fileList.Length);
                if (!_files.ContainsKey(fileList[i]))
                {
                    var file = manager.ReadFile(fileList[i], true);
                    if(file != null)
                        _files.Add(fileList[i], file);
                }
            }

            int position = 1;
            foreach (var kvp in _patchList)
            {
                Console.Title = string.Format("Patching files... {0} / {1}", position, _patchList.Count);
                if(_files.ContainsKey(kvp.Value))
                    manager.WriteFile(kvp.Key, true, _files[kvp.Value], true);
                position++;
            }

            manager.EndWriting();
        }

        private void AddSkinChange(Skin orignal, Skin target)
        {
            AddChangeToList(orignal.BlndFile, target.BlndFile);
            AddChangeToList(orignal.DdsFile, target.DdsFile);
            AddChangeToList(orignal.SknFile, target.SknFile);
            AddChangeToList(orignal.SklFile, target.SklFile);
        }

        private void AddChangeToList(string key, string value)
        {
            if (!_patchList.ContainsKey(key))
                _patchList.Add(key, value);
        }
    }

    public class ScramblerChangeArray
    {
        public string[] Original { get; set; }
        public string[] Scrambled { get; set; }

        public ScramblerChangeArray(string[] original)
        {
            Original = original;
        }
    }
}
