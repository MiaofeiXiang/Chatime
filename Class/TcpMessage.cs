using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Chatime.Class
{
    /// <summary>
    /// Version:1.0
    /// Date:2013/07/28
    /// Author:Miaofei Xiang
    /// <para>Define the TcpMessage data field and a method generating FileInfo preBuffer</para>
    /// </summary>
    public class TcpMessage
    {
        public string filePath
        {
            get;
            protected set;
        }
        public string fileName
        {
            get;
            protected set;
        }
        private byte[] FileInfobuffer;

        public byte[] fileinfobuffer
        {
            get { return FileInfobuffer; }
        }

        public TcpMessage(string filePath)
        {
            this.filePath = filePath;
            this.fileName = Path.GetFileName(filePath);
            FileInfobufferGenerate();
        }
        /// <summary>
        /// Generate file information preBuffer
        /// </summary>
        /// <remarks>The preBuffer is a byte array in format:
        /// <para>[filenamelength][filename][filelength]</para>
        /// <para>filenamelength occupies 1 byte, filelength occupies 8 bytes</para>
        /// </remarks>
        private void FileInfobufferGenerate()
        {
            FileInfo f = new FileInfo(filePath);
            long fileLen = f.Length;
            byte[] fileLenlong = BitConverter.GetBytes(fileLen);
            byte[] fileNamebyte = Encoding.UTF8.GetBytes(fileName);
            int fileInfoLen = 15 + fileNamebyte.Length;
            FileInfobuffer = new byte[fileInfoLen];
            byte[] buffertemp = new byte[5 + fileNamebyte.Length];
            buffertemp[0] = (byte)'[';
            buffertemp[1] = (byte)fileNamebyte.Length;
            buffertemp[2] = (byte)']';
            buffertemp[3] = (byte)'[';
            fileNamebyte.CopyTo(buffertemp, 4);
            buffertemp[4 + fileNamebyte.Length] = (byte)']';
            buffertemp.CopyTo(FileInfobuffer, 0);
            FileInfobuffer[5 + fileNamebyte.Length] = (byte)'[';
            fileLenlong.CopyTo(FileInfobuffer, 6 + fileNamebyte.Length);
            FileInfobuffer[fileInfoLen - 1] = (byte)']';
        }
    }
}
