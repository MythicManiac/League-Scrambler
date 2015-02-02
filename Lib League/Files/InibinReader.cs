using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace League.Files
{
    public class InibinReader
    {

        private Inibin _inibin;
        private BinaryReader _reader;

        private byte[] _data;
        private uint _contentLength;
        private BitArray _format;

        public InibinReader()
        {

        }

        public Inibin DeserializeInibin(byte[] data)
        {
            _data = data;
            _inibin = new Inibin();
            _inibin.Content = new Dictionary<uint, object>();
            _reader = new BinaryReader(new MemoryStream(_data));
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            _inibin.Version = _reader.ReadByte();
            _contentLength = _reader.ReadUInt16();

            if (_inibin.Version != 2)
                throw new InvalidDataException("Wrong Inibin version");

            _format = new BitArray(new byte[] { _reader.ReadByte(), _reader.ReadByte() });

            for (int i = 0; i < _format.Length; i++)
            {
                if (_format[i])
                {
                    if (!DeserializeSegment(i))
                        return null;
                }
            }

            return _inibin;
        }

        private bool DeserializeSegment(int type, bool skipErrors = true)
        {
            int count = _reader.ReadUInt16();
            uint[] keys = GetKeys(count);
            dynamic[] values = new dynamic[count];

            if (type == 0) // Unsigned integers
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadUInt32();
                }
            }
            else if (type == 1) // Floats
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadSingle();
                }
            }
            else if (type == 2) // One byte floats - Divide the byte by 10
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = (float)(_reader.ReadByte() * 0.1f);
                }
            }
            else if (type == 3) // Unsigned shorts
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadUInt16();
                }
            }
            else if (type == 4) // Bytes
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadByte();
                }
            }
            else if (type == 5) // Booleans
            {
                byte[] bytes = new byte[(int)Math.Ceiling((decimal)count / 8)];
                _reader.BaseStream.Read(bytes, 0, bytes.Length);
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
                    _reader.BaseStream.Read(bytes, 0, bytes.Length);
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
                    _reader.ReadInt32();
                    _reader.ReadInt32();
                    _reader.ReadInt32();
                    values[i] = "NotYetImplemented";
                }
            }
            else if (type == 8) // 1x short ??????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadUInt16();
                }
            }
            else if (type == 9) // 2x float ??????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadUInt64();
                }
            }
            else if (type == 10) // 4x bytes * 0.1f ?????
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = _reader.ReadUInt32();
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
                    _reader.ReadUInt64();
                    _reader.ReadUInt64();
                    values[i] = "NotYetImplemented";
                }
            }
            else if (type == 12) // Unsigned short - string dictionary offsets
            {
                long stringListOffset = _reader.BaseStream.Length - _contentLength;

                for (int i = 0; i < count; i++)
                {
                    int offset = _reader.ReadInt16();
                    values[i] = ReadString(stringListOffset + offset);
                }
            }
            else
            {
                if (!skipErrors)
                    throw new Exception("Unknown segment type");

                Console.WriteLine(string.Format("Unknown segment type found in file {0}", _inibin.FilePath));
                return false;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                _inibin.Content.Add(keys[i], values[i]);
            }

            return true;
        }

        private uint[] GetKeys(int count)
        {
            uint[] result = new uint[count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _reader.ReadUInt32();
            }

            return result;
        }

        private string ReadString(long offset)
        {
            long oldPosition = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            string result = "";
            int character = _reader.ReadByte();
            while (character > 0)
            {
                result += (char)character;
                character = _reader.ReadByte();
            }

            _reader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);

            return result;
        }
    }
}
