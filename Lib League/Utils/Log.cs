using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace League.Utils
{
    public class Log : TextWriter
    {
        public string Filename { get; private set; }

        private FileStream _logWriter;
        private TextWriter _originalWriter;

        public Log(string filename)
        {
            Filename = filename;
            _originalWriter = Console.Out;
            _logWriter = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write);
            _logWriter.Seek(0, SeekOrigin.End);
        }

        public void LogLine(string format, params object[] args)
        {
            string value = string.Format(format, args);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override Encoding Encoding
        {
            get { return new System.Text.ASCIIEncoding(); }
        }

        public override void WriteLine(string message)
        {
            _originalWriter.WriteLine(message);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(message + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(int value)
        {
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(Convert.ToString(value) + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(float value)
        {
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(Convert.ToString(value) + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(uint value)
        {
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(Convert.ToString(value) + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(bool value)
        {
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(Convert.ToString(value) + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void Write(int value)
        {
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(string format, object arg0)
        {
            string value = string.Format(format, arg0);
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            string value = string.Format(format, arg0, arg1);
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            string value = string.Format(format, arg0, arg1, arg2);
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        public override void WriteLine(string format, params object[] args)
        {
            string value = string.Format(format, args);
            _originalWriter.WriteLine(value);

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value + Environment.NewLine);
            _logWriter.Write(bytes, 0, bytes.Length);
            _logWriter.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _logWriter.Close();
            base.Dispose(disposing);
        }
    }
}
