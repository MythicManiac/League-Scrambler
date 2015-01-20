using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zlib;
using League.Files;
using League.Files.Manifest;

namespace League.Tools
{
    public class Patcher
    {
        public static readonly string BackupPath = @"C:\LoLBackup\";

        public List<Archive> Archives { get; private set; }
        public string ArchiveLocation { get; private set; }

        public Dictionary<string, byte[]> FileChanges { get; private set; }
        public ReleaseManifest Manifest { get; private set; }
        public List<string> FileRequestTable { get; private set; }

        public Patcher(string archivePath, string manifestPath)
        {
            ArchiveLocation = archivePath;
            FileChanges = new Dictionary<string, byte[]>();
            FileRequestTable = new List<string>();
            Manifest = ReleaseManifest.LoadFromFile(manifestPath);
        }

        public void Backup(bool force = false, bool skipErros = true)
        {
            Archives = new List<Archive>();
            ScanFiles(ArchiveLocation, 1);

            Console.Title = string.Format("Backing up archives... {0} / {1} done", 0, Archives.Count);
            for (int i = 0; i < Archives.Count; i++)
            {
                string backupPath = BackupPath + Archives[i].RafPath.Remove(0, ArchiveLocation.Length);

                if(!Directory.Exists(Path.GetDirectoryName(backupPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath));

                if (File.Exists(backupPath) || File.Exists(backupPath + ".dat"))
                {
                    if(!force && !skipErros)
                        throw new Exception("A backup already exists");

                    if (!force && skipErros)
                        continue;

                    if (force)
                    {
                        File.Delete(backupPath);
                        File.Delete(backupPath + ".dat");
                    }
                }

                File.Copy(Archives[i].RafPath, backupPath);
                File.Copy(Archives[i].RafPath + ".dat", backupPath + ".dat");
                Console.Title = string.Format("Backing up archives... {0} / {1} done", i + 1, Archives.Count);
            }

            if (File.Exists(BackupPath + "manifest") && force)
                File.Delete(BackupPath + "manifest");

            File.Copy(Manifest.FileLocation, BackupPath + "manifest");
        }

        public void Restore(bool removeBackup = true)
        {
            Archives = new List<Archive>();
            ScanFiles(BackupPath, 1, false);

            Console.Title = string.Format("Restoring archives... {0} / {1} done", 0, Archives.Count);
            for (int i = 0; i < Archives.Count; i++)
            {
                string destination = ArchiveLocation + Archives[i].RafPath.Remove(0, BackupPath.Length);

                File.Delete(destination);
                File.Delete(destination + ".dat");
                File.Copy(Archives[i].RafPath, destination);
                File.Copy(Archives[i].RafPath + ".dat", destination + ".dat");
                if (removeBackup)
                {
                    File.Delete(Archives[i].RafPath);
                    File.Delete(Archives[i].RafPath + ".dat");
                }
                Console.Title = string.Format("Restoring archives... {0} / {1} done", i + 1, Archives.Count);
            }

            if (File.Exists(Manifest.FileLocation) && File.Exists(BackupPath + "manifest"))
                File.Delete(Manifest.FileLocation);

            File.Copy(BackupPath + "manifest", Manifest.FileLocation);
        }

        public void Patch()
        {
            Manifest.SaveChanges();
            for(int i = 0; i < Archives.Count; i++)
            {
                Archives[i].PatchFiles(FileChanges);
                Console.Title = string.Format("Patching files... {0} / {1}", i + 1, Archives.Count);
            }
        }

        public void AddFileChange(string key, string file)
        {
            key = key.Replace('\\', '/');
            int fileId = -1;
            for (int i = 0; i < Manifest.Files.Length; i++)
            {
                if (Manifest.Files[i].FullName == key)
                    fileId = i;
            }
            if (fileId > -1)
            {
                if (!File.Exists(file))
                    throw new Exception("Invalid file path provided");

                byte[] data = File.ReadAllBytes(file);
                Manifest.Files[fileId].Descriptor.DecompressedSize = (uint)data.Length;
                data = ZlibStream.CompressBuffer(data);
                Manifest.Files[fileId].Descriptor.CompressedSize = (uint)data.Length;

                FileChanges.Add(key, data);
                
            }
            else
                throw new Exception("Invalid key");
        }

        public void AddFileChange(string key, byte[] file)
        {
            int fileId = -1;
            for (int i = 0; i < Manifest.Files.Length; i++)
            {
                if (Manifest.Files[i].FullName == key)
                    fileId = i;
            }
            if (fileId > -1)
            {
                bool shouldCompress = Manifest.Files[fileId].Descriptor.DecompressedSize > Manifest.Files[fileId].Descriptor.CompressedSize;
                bool shouldCompress2 = Manifest.Files[fileId].Descriptor.DecompressedSize != Manifest.Files[fileId].Descriptor.CompressedSize;

                if ((file[0] == 0x78 && (file[1] == 0x01 || file[1] == 0x9C || file[1] == 0xDA)))
                {
                    Manifest.Files[fileId].Descriptor.CompressedSize = (uint)file.Length;
                    byte[] temp = ZlibStream.UncompressBuffer(file);
                    Manifest.Files[fileId].Descriptor.DecompressedSize = (uint)temp.Length;
                    if (!shouldCompress)
                        file = temp;
                }
                else
                {
                    Manifest.Files[fileId].Descriptor.DecompressedSize = (uint)file.Length;
                    byte[] temp = ZlibStream.CompressBuffer(file);
                    Manifest.Files[fileId].Descriptor.CompressedSize = (uint)temp.Length;
                    if (shouldCompress)
                        file = temp;
                }

                if (!shouldCompress || !shouldCompress2)
                    Console.WriteLine("Shouldn't compress: {0} - {1}", shouldCompress, shouldCompress2);

                FileChanges.Add(key, file);

            }
            else
                throw new Exception("Invalid key");
        }

        public void AddFileRequest(string key)
        {
            int fileId = -1;
            for (int i = 0; i < Manifest.Files.Length; i++)
            {
                if (Manifest.Files[i].FullName == key)
                    fileId = i;
            }
            if (fileId > -1)
            {
                if(!FileRequestTable.Contains(key))
                    FileRequestTable.Add(key);
            }
            else
                throw new Exception("Invalid key");
        }

        public Dictionary<string, byte[]> ReadFiles()
        {
            Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();

            Console.Title = string.Format("Searching files... 0 / {0}", FileRequestTable.Count);
            for (int i = 0; i < FileRequestTable.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < Archives.Count; j++)
                {
                    for (int k = 0; k < Archives[j].Files.Count; k++)
                    {
                        if (Archives[j].Files[k].PathMetadata.Path == FileRequestTable[i])
                        {
                            result.Add(FileRequestTable[i], Archives[j].GetFile(k));
                            found = true;
                            break;
                        }
                    }
                    if(found)
                        break;
                }
                if(!found)
                    Console.WriteLine("Couldn't find the file with key {0}", FileRequestTable[i]);

                Console.Title = string.Format("Searching files... {0} / {1}", i, FileRequestTable.Count);
            }

            return result;
        }

        public void LoadArchives()
        {
            Console.Title = string.Format("Searching for archives...");
            Archives = new List<Archive>();
            ScanFiles(ArchiveLocation, 1);

            for(int i = 0; i < Archives.Count; i++)
            {
                Archives[i].Read();
            }
        }

        private void ScanFiles(string path, int depth, bool reguireData = true)
        {
            // Check if there are any .raf files inside this folder
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == ".raf" && (File.Exists(files[i] + ".dat") || !reguireData))
                {
                    Archives.Add(new Archive(files[i]));
                    Console.Title = string.Format("Searching for archives... {0} found", Archives.Count);
                }
            }

            // Scan all the subdirectories as well
            if (depth > 0)
            {
                string[] directories = Directory.GetDirectories(path);
                for (int i = 0; i < directories.Length; i++)
                {
                    ScanFiles(directories[i], depth - 1, reguireData);
                }
            }
        }
    }
}
