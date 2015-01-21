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
        private string[] _squareIconTable;
        private string[] _squareIconTableScrambled;
        private string[] _loadingScreenTable;
        private string[] _loadingScreenTableScrambled;
        private string[] _circleIconTable;
        private string[] _circleIconTableScrambled;
        private string[] _abilityIconTable;
        private string[] _abilityIconTableScrambled;
        private string[] _itemIconTable;
        private string[] _itemIconTableScrambled;

        private Dictionary<string, string> _skinChangeTable;

        public Scrambler(string leaguepath)
        {
            LeaguePath = leaguepath;
        }

        public void Initialize()
        {
            SkinGroupReader reader = new SkinGroupReader();
            PathListReader pr = new PathListReader();
            _skinTable = reader.Read(Properties.Resources.SkinGroups);
            _squareIconTable = pr.Read(Properties.Resources.SquareIconPaths);
            _circleIconTable = pr.Read(Properties.Resources.CircleIconPaths);
            _abilityIconTable = pr.Read(Properties.Resources.AbilityIconPaths);
            _loadingScreenTable = pr.Read(Properties.Resources.LoadScreenPaths);
            _itemIconTable = pr.Read(Properties.Resources.ItemIconPaths);
        }

        public void Scramble()
        {
            // Generate an index table we can remove stuff out of
            List<int> freeIndexes = new List<int>();
            for(int i = 0; i < _skinTable.Length; i++)
            {
                freeIndexes.Add(i);
            }

            
            // Fill and scramble the second table
            _skinTableScrambled = new SkinGroup[_skinTable.Length];
            Random random = new Random();
            for(int i = 0; i < _skinTableScrambled.Length; i++)
            {
                int index = random.Next(0, freeIndexes.Count); 
                _skinTableScrambled[i] = _skinTable[freeIndexes[index]];
                freeIndexes.RemoveAt(index);
            }

            ScrambleArrays(_squareIconTable, ref _squareIconTableScrambled);
            ScrambleArrays(_circleIconTable, ref _circleIconTableScrambled);
            ScrambleArrays(_abilityIconTable, ref _abilityIconTableScrambled);
            ScrambleArrays(_loadingScreenTable, ref _loadingScreenTableScrambled);
            ScrambleArrays(_itemIconTable, ref _itemIconTableScrambled);

            // LEAGUE OF DRAVEN
            /*
            _skinTableScrambled = new SkinGroup[_skinTable.Length];
            for(int i = 0; i < _skinTable.Length; i++)
            {
                if(_skinTable[i].Skins[0].BlndFile.Contains("Draven"))
                {
                    for(int j = 0; j < _skinTableScrambled.Length; j++)
                    {
                        _skinTableScrambled[j] = _skinTable[i];
                    }
                    break;
                }
            }
            */
        }

        private void ScrambleArrays(string[] table1, ref string[] table2)
        {
            // Generate an index table we can remove stuff out of
            List<int> freeIndexes = new List<int>();
            for (int i = 0; i < table1.Length; i++)
            {
                freeIndexes.Add(i);
            }


            // Fill and scramble the second table
            table2 = new string[table1.Length];
            Random random = new Random();
            for (int i = 0; i < table2.Length; i++)
            {
                int index = random.Next(0, freeIndexes.Count);
                table2[i] = table1[freeIndexes[index]];
                freeIndexes.RemoveAt(index);
            }
        }

        public void Prepare()
        {
            _skinChangeTable = new Dictionary<string, string>();

            // Prepare our changes for the patcher
            for(int i = 0; i < _skinTable.Length; i++)
            {
                for(int j = 0, k = 0; j < _skinTable[i].Skins.Count; j++)
                {
                    AddSkinChange(_skinTable[i].Skins[j], _skinTableScrambled[i].Skins[k]);

                    // Loop the target sking group indexer around in case the new skin group has less skins than the original
                    k++;
                    if (k == _skinTableScrambled[i].Skins.Count)
                        k = 0;
                }
            }
        }

        public void AddRequests(string[] files, Patcher patcher)
        {
            for(int i = 0; i < files.Length; i++)
            {
                patcher.AddFileRequest(files[i]);
                Console.Title = string.Format("{0} / {1}", i + 1, files.Length);
            }
        }

        public void Patch()
        {
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(LeaguePath), LeagueLocations.GetManifestPath(LeaguePath), LeagueLocations.GetBackupPath(LeaguePath));
            patcher.LoadArchives();
            int i = 1;
            foreach(KeyValuePair<string, string> kvp in _skinChangeTable)
            {
                Console.Title = string.Format("Adding files to request list... {0} / {1}", i, _skinChangeTable.Count);
                patcher.AddFileRequest(kvp.Value);
                i++;
            }

            AddRequests(_abilityIconTableScrambled, patcher);
            AddRequests(_loadingScreenTableScrambled, patcher);
            AddRequests(_itemIconTableScrambled, patcher);
            AddRequests(_squareIconTableScrambled, patcher);
            AddRequests(_circleIconTableScrambled, patcher);

            Dictionary<string, byte[]> files = patcher.ReadFiles();
            i = 0;
            /*
            foreach(KeyValuePair<string, byte[]> kvp in files)
            {
                if (!System.IO.Directory.Exists(@"C:\test\" + System.IO.Path.GetDirectoryName(kvp.Key)))
                    System.IO.Directory.CreateDirectory(@"C:\test\" + System.IO.Path.GetDirectoryName(kvp.Key));

                System.IO.File.WriteAllBytes(@"C:\test\" + kvp.Key, ZlibStream.UncompressBuffer(kvp.Value));
                Console.Title = string.Format("{0} / {1}", i, files.Count);
                i++;
            }
            */
            
            i = 1;
            foreach(KeyValuePair<string, string> kvp in _skinChangeTable)
            {
                Console.Title = string.Format("Adding files to patch list... {0} / {1}", i, _skinChangeTable.Count);
                patcher.AddFileChange(kvp.Key, files[kvp.Value]);
                i++;
            }

            AddFileChangeTable(_abilityIconTable, _abilityIconTableScrambled, files, patcher);
            AddFileChangeTable(_loadingScreenTable, _loadingScreenTableScrambled, files, patcher);
            AddFileChangeTable(_itemIconTable, _itemIconTableScrambled, files, patcher);
            AddFileChangeTable(_squareIconTable, _squareIconTableScrambled, files, patcher);
            AddFileChangeTable(_circleIconTable, _circleIconTableScrambled, files, patcher);

            patcher.Patch();
        }

        public void AddFileChangeTable(string[] keys, string[] values, Dictionary<string, byte[]> data, Patcher patcher)
        {
            if (keys.Length != values.Length)
                throw new Exception("Keys and values must be of equal length");

            for (int i = 0; i < keys.Length; i++)
            {
                if(data.ContainsKey(values[i]))
                    patcher.AddFileChange(keys[i], data[values[i]]);
                Console.Title = string.Format("{0} / {1}", i + 1, keys.Length);
            }
        }

        public void Dump()
        {
            Table table = new Table(2);
            foreach(KeyValuePair<string, string> kvp in _skinChangeTable)
            {
                table.AddRow(kvp.Key, kvp.Value);
            }
            table.DumpTable(6);
        }

        private void AddSkinChange(Skin orignal, Skin target)
        {
            AddChangeToTable(orignal.BlndFile, target.BlndFile);
            AddChangeToTable(orignal.DdsFile, target.DdsFile);
            AddChangeToTable(orignal.SknFile, target.SknFile);
            AddChangeToTable(orignal.SklFile, target.SklFile);
        }

        private void AddChangeToTable(string key, string value)
        {
            if (!_skinChangeTable.ContainsKey(key))
                _skinChangeTable.Add(key, value);
        }
    }
}
