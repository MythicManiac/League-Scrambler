using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueCommon.Files
{
    public class Inibin
    {
        public string InibinPath { get; private set; }
        public string RootPath { get; private set; }
        public byte Version { get; private set; }
        public Dictionary<uint, object> Data { get; private set; }

        private uint _contentLength;
        private BitArray _format;
        private BinaryReader _stream;

        public Inibin(string path, string rootPath = "")
        {
            if (Path.GetExtension(path) != ".inibin")
                throw new Exception("Must provide a path to an .inibin file");

            InibinPath = path;
            RootPath = rootPath;
        }

        public string LeaguePath
        {
            get
            {
                return InibinPath.Replace(RootPath, "").Replace('\\', '/');
            }
        }

        public bool Read()
        {
            Data = new Dictionary<uint, object>();
            FileStream fileStream = File.OpenRead(InibinPath);
            _stream = new BinaryReader(fileStream);

            Version = _stream.ReadByte();
            _contentLength = _stream.ReadUInt16();

            if (Version != 2)
                throw new InvalidDataException("Wrong Inibin version");

            _format = new BitArray(new byte[] { _stream.ReadByte(), _stream.ReadByte() });

            for (int i = 0; i < _format.Length; i++)
            {
                if(_format[i])
                {
                    if (!ReadSegment(i))
                        return false;
                }
            }

            return true;
        }

        public void ListData()
        {
            foreach (KeyValuePair<uint, object> item in Data)
            {
                Console.WriteLine(string.Format("{0} - {1}", item.Key, item.Value));
            }
        }

        private bool ReadSegment(int type, bool skipErrors = true)
        {
            int count = _stream.ReadUInt16();
            uint[] keys = GetKeys(count);
            dynamic[] values = new dynamic[count];

            if(type == 0) // Unsigned integers
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadUInt32();
                }
            }
            else if (type == 1) // Floats
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadSingle();
                }
            }
            else if (type == 2) // One byte floats - Divide the byte by 10
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = (float)(_stream.ReadByte() * 0.1f);
                }
            }
            else if (type == 3) // Unsigned shorts
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadUInt16();
                }
            }
            else if (type == 4) // Bytes
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadByte();
                }
            }
            else if (type == 5) // Booleans
            {
                byte[] bytes = new byte[(int)Math.Ceiling((decimal)count / 8)];
                _stream.BaseStream.Read(bytes, 0, bytes.Length);
                BitArray bits = new BitArray(bytes);

                for (int i = 0; i < count; i++)
                {
                    values[i] = bits[i];
                }
            }
            else if (type == 6) // 3x byte values - Resource bar RGB color
            {
                byte[] bytes = new byte[3];

                for (int i = 0; i < count; i++)
                {
                    _stream.BaseStream.Read(bytes, 0, bytes.Length);
                    values[i] = BitConverter.ToUInt32(new byte[4] { 0, bytes[0], bytes[1], bytes[2] }, 0);
                }
            }
            else if (type == 7) // 3x float values ??????
            {
                if (!skipErrors)
                    throw new Exception("Reading 12 byte values not yet supported");

                //Console.WriteLine("Tried to read 12 byte values");
                for (int i = 0; i < count; i++)
                {
                    // 4 + 4 + 4 = 12
                    _stream.ReadInt32();
                    _stream.ReadInt32();
                    _stream.ReadInt32();
                    values[i] = "NotYetImplemented";
                }
            }
            else if (type == 8) // 1x short ??????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadUInt16();
                }
            }
            else if (type == 9) // 2x float ??????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadUInt64();
                }
            }
            else if (type == 10) // 4x bytes * 0.1f ?????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _stream.ReadUInt32();
                }
            }
            else if (type == 11) // 4x float ?????
            {
                if (!skipErrors)
                    throw new Exception("Reading 16 byte values not yet supported");

                //Console.WriteLine("Tried to read 16 byte values");
                for (int i = 0; i < count; i++)
                {
                    // 8 + 8 = 16
                    _stream.ReadUInt64();
                    _stream.ReadUInt64();
                    values[i] = "NotYetImplemented";
                }
            }
            else if (type == 12) // Unsigned short - string dictionary offsets
            {
                long stringListOffset = _stream.BaseStream.Length - _contentLength;

                for (int i = 0; i < count; i++)
                {
                    int offset = _stream.ReadInt16();
                    values[i] = ReadString(stringListOffset + offset);
                }
            }
            else
            {
                if(!skipErrors)
                    throw new Exception("Unknown segment type");

                Console.WriteLine(string.Format("Unknown segment type found in file {0}", LeaguePath));
                return false;
            }

            for(int i = 0; i < keys.Length; i++)
            {
                Data.Add(keys[i], values[i]);
            }

            return true;
        }

        private uint[] GetKeys(int count)
        {
            uint[] result = new uint[count];

            for(int i = 0; i < result.Length; i++)
            {
                result[i] = _stream.ReadUInt32();
            }

            return result;
        }

        private string ReadString(long offset)
        {
            long oldPosition = _stream.BaseStream.Position;
            _stream.BaseStream.Seek(offset, SeekOrigin.Begin);

            string result = "";
            int character = _stream.ReadByte();
            while(character > 0)
            {
                result += (char)character;
                character = _stream.ReadByte();
            }

            _stream.BaseStream.Seek(oldPosition, SeekOrigin.Begin);

            return result;
        }
    }
}
