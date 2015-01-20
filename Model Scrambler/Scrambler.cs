using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueCommon;
using LeagueCommon.Tools;
using LeagueCommon.Utils;

namespace LeagueScrambler
{
    public class Scrambler
    {
        public string LeaguePath { get; set; }

        private SkinGroup[] _skinTable;
        private SkinGroup[] _skinTableScrambled;
        private Dictionary<string, string> _skinChangeTable;

        public Scrambler(string leaguepath)
        {
            LeaguePath = leaguepath;
        }

        public void Initialize()
        {
            SkinGroupReader reader = new SkinGroupReader();
            _skinTable = reader.Read(Properties.Resources.SkinGroups);
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

        public void Patch()
        {
            Patcher patcher = new Patcher(LeagueLocations.GetArchivePath(LeaguePath), LeagueLocations.GetManifestPath(LeaguePath));
            patcher.LoadArchives();
            int i = 1;
            foreach(KeyValuePair<string, string> kvp in _skinChangeTable)
            {
                Console.Title = string.Format("Adding files to request list... {0} / {1}", i, _skinChangeTable.Count);
                patcher.AddFileRequest(kvp.Value);
                i++;
            }
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
            patcher.Patch();
        }

        public void Dump()
        {
            LeagueCommon.Utils.Table table = new LeagueCommon.Utils.Table(2);
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
