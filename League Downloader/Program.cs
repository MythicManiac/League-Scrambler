using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.IO;
using League;
using League.Utils;
using League.Tools;
using League.Files;
using League.Files.Manifest;

namespace League_Downloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.SetOut(new Log("Output.txt"));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Run();
            stopwatch.Stop();
            Console.WriteLine(string.Format("Operation completed in {0} milliseconds", stopwatch.ElapsedMilliseconds));
            Console.ReadKey();
        }

        public static void Run()
        {
            //var manager = new ArchiveFileManager(LeagueLocations.GetLeaguePath());

            //DownloadFiles();
            Console.ForegroundColor = ConsoleColor.White;
            //FormInstallation();
            FormSolution();
        }

        public static void FormSolution()
        {
            //var version = "0.0.1.68";
            var solution_dir = "temp/RADS/solutions/lol_game_client_sln/releases/0.0.1.68";
            var gamedir = "temp/RADS/projects/lol_game_client/releases/0.0.1.7/deploy";

            CopyDirectory(gamedir, solution_dir + "/deploy");

            File.WriteAllBytes(solution_dir + "/solutionmanifest", Properties.Resources.solutionmanifest);
            File.WriteAllBytes(solution_dir + "/configurationmanifest", Properties.Resources.configurationmanifest);
        }

        public static void CopyDirectory(string source, string target)
        {
            var dir = new DirectoryInfo(source);
            var subdirs = dir.GetDirectories();

            if (!Directory.Exists(target)) { Directory.CreateDirectory(target); }

            var files = dir.GetFiles();
            foreach(var file in files)
            {
                var path = Path.Combine(target, file.Name);
                file.CopyTo(path, true);
            }

            foreach(var subdir in subdirs)
            {
                string path = Path.Combine(target, subdir.Name);
                CopyDirectory(subdir.FullName, path);
            }
        }

        public static void FormInstallation()
        {
            if (!Directory.Exists("temp/download/projects/lol_game_client/releases")) { return; }

            var version = "0.0.1.7";
            var base_dir = "temp/RADS";
            var release_dir = base_dir + "/projects/lol_game_client/releases/" + version;
            var base_file_dir = release_dir + "/deploy";
            var versions = Directory.GetDirectories("temp/download/projects/lol_game_client/releases");
            var archive_base_dir = base_dir + "/projects/lol_game_client/filearchives";
            var base_files = new List<string>();
            //var release_files = new Dictionary<string, Archive>();
            File.WriteAllBytes(release_dir + "/releasemanifest", Properties.Resources.releasemanifest);
            var release_manifest = ReleaseManifest.LoadFromFile(release_dir + "/releasemanifest");
            var release_files = new Dictionary<string, ReleaseManifestFileEntry>();
            foreach (var file in release_manifest.Files)
            {
                release_files.Add(file.FullName, file);
            }

            var count = 1;
            var skipped = 0;
            var not_skipped = 0;

            // Form archives and sort out base files
            foreach (var directory in versions)
            {
                var archive_version = directory.Replace('\\', '/').Split('/').Last();
                var archive = Archive.CreateArchive(archive_base_dir + "/" + archive_version + "/", 1);
                var archive_data = new MemoryStream();

                Console.Title = string.Format("Forming archives... {0} / {1}", count, versions.Length);
                Console.WriteLine("Forming archive {0}", archive.FilePath);

                var filedir = directory + "/files";
                var files = Directory.GetFiles(filedir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var path = file.Replace('\\', '/');
                    var ritopath = file.Substring(file.IndexOf("/files\\") + 7).Replace('\\', '/');
                    bool compress = path.Split('.').Last().ToLower() == "compressed";
                    if(compress)
                    {
                        ritopath = ritopath.Substring(0, ritopath.LastIndexOf('.'));
                    }

                    if (!ritopath.Contains('/') || ritopath == "DATA/Images/SplashScreen.dds")
                    {
                        base_files.Add(path);
                    }
                    else
                    {
                        ArchiveFile archive_file = new ArchiveFile(File.ReadAllBytes(path), compress);
                        ArchiveFileInfo archive_file_info = new ArchiveFileInfo();
                        archive_file_info.DataOffset = (uint)archive_data.Position;
                        archive_file_info.DataLength = (uint)archive_file.Data.Length;
                        archive_file_info.Path = ritopath;
                        archive.Files.Add(ritopath, archive_file_info);
                        archive_data.Write(archive_file.Data, 0, archive_file.Data.Length);
                        if (release_files.ContainsKey(ritopath))
                        {
                            release_files[ritopath].Descriptor.ArchiveId = archive.ArchiveIndex;
                            release_files[ritopath].Descriptor.CompressedSize = archive_file.CompressedSize;
                            release_files[ritopath].Descriptor.DecompressedSize = archive_file.UncompressedSize;
                            //release_files.Add(ritopath, archive);
                            not_skipped++;
                        }
                        else
                        {
                            skipped++;
                            Console.WriteLine("Skipped a file. Skipped: {0} - Not skipped: {1}", skipped, not_skipped);
                        }
                    }
                }

                File.WriteAllBytes(archive.DataFilePath, archive_data.ToArray());
                archive.SaveChanges();
                count++;
            }

            if (!Directory.Exists(base_file_dir)) { Directory.CreateDirectory(base_file_dir); }


            //var version_number = BitConverter.ToUInt32(new byte[4] { 7, 1, 0, 0 }, 0);
            //ReleaseManifest.Generate(release_dir + "/releasemanifest", release_files, version_number);

            foreach (var filepath in base_files)
            {
                var compress = filepath.Split('.').Last().ToLower() == "compressed";
                var filename = filepath.Split('/').Last();

                if (compress) { filename = filename.Substring(0, filename.LastIndexOf('.')); }

                var file = new ArchiveFile(File.ReadAllBytes(filepath), compress);
                File.WriteAllBytes(base_file_dir + "/" + filename, file.Uncompress());
            }
        }

        public static void DownloadFiles()
        {
            var web = new WebClient();

            var version = "0.0.1.7";
            var base_url = "http://l3cdn.riotgames.com/releases/live";
            var url = string.Format("{0}/projects/lol_game_client/releases/{1}/packages/files/packagemanifest", base_url, version);

            var package_manifest = web.DownloadData(url);

            var manifest_dir = "temp";
            var download_dir = "temp/download/";
            if (!Directory.Exists(manifest_dir)) { Directory.CreateDirectory(manifest_dir); }

            File.WriteAllBytes(manifest_dir + @"\packagemanifest", package_manifest);

            var download_list = new List<string>();
            var lines = File.ReadAllLines(manifest_dir + @"\packagemanifest");
            foreach (var line in lines)
            {
                if (line.Substring(0, 3).ToLower() == "pkg") { continue; }

                download_list.Add(line.Substring(0, line.IndexOf(',')));
            }

            Console.Title = string.Format("Downloading files... {0} / {1}", 0, download_list.Count);
            for (int i = 0; i < download_list.Count; i++)
            {
                Console.Title = string.Format("Downloading files... {0} / {1}", i + 1, download_list.Count);

                var directory_name = download_dir + download_list[i].Substring(0, download_list[i].LastIndexOf('/'));

                if (!Directory.Exists(directory_name)) { Directory.CreateDirectory(directory_name); }

                web.DownloadFile(base_url + download_list[i], download_dir + download_list[i]);
            }
        }
    }
}
