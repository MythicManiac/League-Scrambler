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
        private Dictionary<string, byte[]> _files;

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

        public void Scramble(Settings settings)
        {
            for (int i = 0; i < _changes.Count; i++)
            {
                _changes[i].Scrambled = ScrambleArray(_changes[i].Original);
            }
            _skinTableScrambled = ScrambleArray(_skinTable);

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

        private T[] ScrambleArray<T>(T[] input)
        {
            // Generate an index table we can remove stuff out of
            List<int> freeIndexes = new List<int>();
            for (int i = 0; i < input.Length; i++)
            {
                freeIndexes.Add(i);
            }

            // Fill and scramble the second table
            var result = new T[input.Length];
            Random random = new Random();
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
            if (manager.ArchivesModified)
            {
                Console.Title = "Reverting previous modifications...";
                manager.Revert();
            }

            _files = new Dictionary<string, byte[]>();
            var fileList = _patchList.Values.ToArray();

            for (int i = 0; i < fileList.Length; i++)
            {
                Console.Title = string.Format("Fetching files... {0} / {1}", i + 1, fileList.Length);
                if (!_files.ContainsKey(fileList[i]))
                {
                    var file = manager.ReadFile(fileList[i], false, true);
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

            manager.WriteStateInfo();
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
