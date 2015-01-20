using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zlib;

namespace LeagueCommon.Files
{
    // This class has been written with the help of the following page: http://leagueoflegends.wikia.com/wiki/RAF:_Riot_Archive_File
    public class Archive
    {
        private const int MAGIC_NUMBER = 0x18be0ef0;

        private FileStream rafStream;
        private FileStream rafDataStream;
        private uint fileListOffset;
        private uint pathListOffset;

        public FileEntry[] FileList { get; private set; }
        public PathEntry[] PathList { get; private set; }
        public FilePath[] FilePaths { get; private set; }
        public List<RiotFile> Files { get; private set; }
        public uint Version { get; private set; }
        public uint Index { get; private set; }
        public string RafPath { get; private set; }

        public Archive(string path)
        {
            RafPath = path;
        }

        public void Read()
        {
            if (!File.Exists(RafPath))
                throw new Exception("Specified RAF doesn't exist"); 
            
            rafStream = new FileStream(RafPath, FileMode.Open, FileAccess.Read);

            ReadHeader();
            ReadFileList();
            ReadPathList();
            ReadPaths();
            IndexFiles();

            rafStream.Close();
        }

        public void PatchFiles(Dictionary<string, byte[]> fileChanges)
        {
            Dictionary<string, byte[]> localFileChanges = new Dictionary<string, byte[]>();

            for (int i = 0; i < Files.Count; i++)
            {
                if (fileChanges.ContainsKey(Files[i].PathMetadata.Path))
                    localFileChanges.Add(Files[i].PathMetadata.Path, fileChanges[Files[i].PathMetadata.Path]);
            }

            if (localFileChanges.Count > 0)
            {
                Console.WriteLine(string.Format("Patching file {0}.dat", RafPath));
                PatchDataFile(localFileChanges);
                Console.WriteLine(string.Format("Patching file {0}", RafPath));
                PatchFileList();
            }
        }

        private void PatchDataFile(Dictionary<string, byte[]> changes)
        {
            rafDataStream = new FileStream(RafPath + ".dat", FileMode.Open, FileAccess.ReadWrite);

            for(int i = 0; i < FileList.Length; i++)
            {
                if(changes.ContainsKey(FilePaths[FileList[i].Index].Path))
                {
                    byte[] data = changes[FilePaths[FileList[i].Index].Path];

                    if(data.Length > FileList[i].Size)
                    {
                        Console.WriteLine("Appended {0}", FilePaths[FileList[i].Index].Path);
                        rafDataStream.Seek(0, SeekOrigin.End);
                        FileList[i].Offset = (uint)rafDataStream.Position;
                        rafDataStream.Write(data, 0, data.Length);
                        FileList[i].Size = data.Length;
                    }
                    else
                    {
                        Console.WriteLine("Overwrote {0}", FilePaths[FileList[i].Index].Path);
                        rafDataStream.Seek(FileList[i].Offset, SeekOrigin.Begin);
                        rafDataStream.Write(data, 0, data.Length);
                        FileList[i].Size = data.Length;
                    }
                }
            }

            rafDataStream.Flush();
            rafDataStream.Close();
        }

        private void PatchFileList()
        {
            rafStream = new FileStream(RafPath, FileMode.Open, FileAccess.ReadWrite);
            rafStream.Seek(fileListOffset, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(rafStream);
            writer.Write((uint)FileList.Length);

            for(int i = 0; i < FileList.Length; i++)
            {
                writer.Write(FileList[i].Hash);
                writer.Write(FileList[i].Offset);
                writer.Write((uint)FileList[i].Size);
                writer.Write(FileList[i].Index);
            }

            rafStream.Flush();
            rafStream.Close();
        }

        public void ListFiles()
        {
            if (Files == null)
                throw new Exception("Must read archive before listing files");

            for(int i = 0; i < Files.Count; i++)
            {
                Console.WriteLine(Files[i].PathMetadata.Path);
            }
        }

        public void Extract(string destination)
        {
            if (!File.Exists(RafPath + ".dat"))
                throw new Exception("Specified RAF is missing it's data"); 
            
            if (Files == null)
                throw new Exception("Must read archive before saving files");

            rafDataStream = new FileStream(RafPath + ".dat", FileMode.Open, FileAccess.Read);

            byte[] buffer;
            FileStream writeStream;
            string fullPath;

            for(int i = 0; i < Files.Count; i++)
            {
                // Write status to console
                Console.WriteLine(string.Format("Extracting file {0} - {1} bytes", Files[i].PathMetadata.Path, Files[i].FileMetadata.Size));

                // Get the full path of the file we're going to save
                fullPath = Path.Combine(destination, Files[i].PathMetadata.Path);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                // Create the file
                writeStream = File.Create(fullPath);

                // Read the file in the buffer
                buffer = new byte[Files[i].FileMetadata.Size];
                rafDataStream.Seek(Files[i].FileMetadata.Offset, SeekOrigin.Begin);
                rafDataStream.Read(buffer, 0, Files[i].FileMetadata.Size);

                // Decompress the buffer if needed
                if(buffer[0] == 0x78 && (buffer[1] == 0x01 || buffer[1] == 0x9C || buffer[1] == 0xDA))
                {
                    buffer = ZlibStream.UncompressBuffer(buffer);
                }

                // Write the file
                writeStream.Write(buffer, 0, buffer.Length);
                writeStream.Flush();
                writeStream.Close();
            }
        }

        public byte[] GetFile(int pathId)
        {
            rafDataStream = new FileStream(RafPath + ".dat", FileMode.Open, FileAccess.Read);
            rafDataStream.Seek(Files[pathId].FileMetadata.Offset, SeekOrigin.Begin);
            byte[] data = new byte[Files[pathId].FileMetadata.Size];
            rafDataStream.Read(data, 0, data.Length);
            rafDataStream.Close();
            return data;
        }

        private void ReadHeader()
        {
            byte[] bytes = new byte[4];
            rafStream.Seek(0, SeekOrigin.Begin);

            // Read the magic number, this should always match the constant
            rafStream.Read(bytes, 0, 4);
            if (BitConverter.ToInt32(bytes, 0) != MAGIC_NUMBER)
                throw new Exception("Invalid archive");

            // Read the version of the archive
            rafStream.Read(bytes, 0, 4);
            Version = BitConverter.ToUInt32(bytes, 0);

            // Read the manager index
            rafStream.Read(bytes, 0, 4);
            Index = BitConverter.ToUInt32(bytes, 0);

            // Read the file list offset
            rafStream.Read(bytes, 0, 4);
            fileListOffset = BitConverter.ToUInt32(bytes, 0);

            // Read the path list offset
            rafStream.Read(bytes, 0, 4);
            pathListOffset = BitConverter.ToUInt32(bytes, 0);
        }

        private void ReadFileList()
        {
            byte[] bytes = new byte[16];
            rafStream.Seek(fileListOffset, SeekOrigin.Begin);

            // Read the count of files and create our file list
            rafStream.Read(bytes, 0, 4);
            FileList = new FileEntry[BitConverter.ToUInt32(bytes, 0)];

            // Read all file entries to the file list
            uint hash;
            uint offset;
            int size;
            uint index;

            for(int i = 0; i < FileList.Length; i++)
            {
                // Read the file entry to the buffer
                rafStream.Read(bytes, 0, 16);

                // Path Hash
                hash = BitConverter.ToUInt32(bytes, 0);

                // Data Offset
                offset = BitConverter.ToUInt32(bytes, 4);
                
                // Data Size
                size = BitConverter.ToInt32(bytes, 8);
                
                // Path Index
                index = BitConverter.ToUInt32(bytes, 12);
                
                // Add the file entry to the file list
                FileList[i] = new FileEntry(hash, offset, size, index);
            }
        }

        private void ReadPathList()
        {
            byte[] bytes = new byte[8];
            rafStream.Seek(pathListOffset, SeekOrigin.Begin);

            // Read the path list to the buffer
            rafStream.Read(bytes, 0, 8);

            // Path list size in bytes, currently unused
            uint size = BitConverter.ToUInt32(bytes, 0);

            // Read the amount of path list entries and create our path list by using that information
            PathList = new PathEntry[BitConverter.ToUInt32(bytes, 4)];

            // Read all the path list entries to the path list
            uint offset;
            int length;

            for(int i = 0; i < PathList.Length; i++)
            {
                // Read the path list entry to the buffer
                rafStream.Read(bytes, 0, 8);

                // Path offset - Location of the path is this and path list offset combined
                offset = BitConverter.ToUInt32(bytes, 0);
                //Console.WriteLine("Offset: " + offset);

                // Path length
                length = BitConverter.ToInt32(bytes, 4);

                // Add the path list entry to the path list
                PathList[i] = new PathEntry(offset, length);
            }
        }

        private void ReadPaths()
        {
            FilePaths = new FilePath[PathList.Length];
            byte[] bytes;
            string path;

            for (int i = 0; i < PathList.Length; i++)
            {
                // Position the stream properly and create a properly sized buffer
                rafStream.Seek(pathListOffset + PathList[i].Offset, SeekOrigin.Begin);
                bytes = new byte[PathList[i].Length];

                // Read the path to the buffer and convert it to a string - Remove the last byte because it is always 0x00
                rafStream.Read(bytes, 0, PathList[i].Length);
                path = ASCIIEncoding.ASCII.GetString(bytes, 0, bytes.Length - 1);

                // Add the path to our list
                FilePaths[i] = new FilePath(path, PathHash(path));
            }
        }

        private void IndexFiles()
        {
            Files = new List<RiotFile>(FileList.Length);
            for(int i = 0; i < FileList.Length; i++)
            {
                try
                {
                    Files.Add(new RiotFile(FileList[i], FilePaths[FileList[i].Index]));
                }
                catch
                {
                    Console.WriteLine(string.Format("{0} - Error with file at index {1} - {2} - {3}", RafPath.Remove(0, 70), i, FileList[i].Index, FilePaths[FileList[i].Index].Path));
                }
            }
        }

        // Black magic hash function, copied the pseudocode of the RAF specification found on LoL wiki
        public static uint PathHash(string path)
        {
            uint hash = 0;
            uint temp = 0;

            path = path.ToLower();

            for (int i = 0; i < path.Length; i++)
            {
                hash = (hash << 4) + path[i];
                temp = hash & 0xf0000000;
                if (temp != 0)
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }
            
            return hash;
        }
    }

    public struct FileEntry
    {
        public uint Hash; // Hash of the file's path
        public uint Offset; // Offset in the data file
        public int Size; // Size in bytes
        public uint Index; // Path list index

        public FileEntry(uint hash, uint offset, int size, uint index)
        {
            Hash = hash;
            Offset = offset;
            Size = size;
            Index = index;
        }
    }

    public struct PathEntry
    {
        public uint Offset; // Offset from the path list offset
        public int Length; // Length of the path in bytes

        public PathEntry(uint offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }

    public struct FilePath
    {
        public string Path; // Actual path of the file
        public uint Hash; // Hash of the path

        public FilePath(string path, uint hash)
        {
            Path = path;
            Hash = hash;
        }
    }
}
