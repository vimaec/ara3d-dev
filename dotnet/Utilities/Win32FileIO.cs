using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// These are wrappers around some of the more common Win32 API calls for File IO. This exists 
    /// to move large amounts of binary data efficiently with a minimum of memory copies. 
    /// I was inspired by 
    /// https://stackoverflow.com/questions/18243414/how-to-pass-unsafe-pointer-to-filestream-in-c-sharp
    /// https://designingefficientsoftware.wordpress.com/2011/03/03/efficient-file-io-from-csharp/
    /// </summary>
    public static class Win32FileIO
    {
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;

        public const uint OPEN_EXISTING = 3;
        public const uint CREATE_ALWAYS = 2;

        /// <summary>
        /// Opens or creates a file. When the SafeFileHandle is disposed it closes the file, 
        /// but it does not explicitly flush the buffers. Flushing happens lazily when the
        /// system decides.
        /// https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-createfilea
        /// https://stackoverflow.com/questions/7376956/close-a-filestream-without-flush
        /// https://blogs.msdn.microsoft.com/oldnewthing/20170524-00/?p=96215
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>
        /// Calls the Win32 API for ReadFile returning false if successful. 
        /// https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-readfile
        /// https://www.pinvoke.net/default.aspx/kernel32/readfile.html
        /// </summary>
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool ReadFile
        (
             SafeFileHandle hFile,
             IntPtr pBuffer,
             int nBytesToRead,
             out int nBytesRead,
             IntPtr pOverlapped // ref System.Threading.NativeOverlapped lpOverlapped
        );

        /// <summary>
        /// Calls the Win32 API for WriteFile returning false if successful. 
        /// https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-writefile
        /// http://pinvoke.net/default.aspx/kernel32/WriteFile.html
        /// </summary>
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool WriteFile
        (
            SafeFileHandle hFile,
            IntPtr pBuffer,
            int nBytesToWrite,
            out int nBytesWritten,
            IntPtr pOverlapped // ref System.Threading.NativeOverlapped lpOverlapped
        );

        /// <summary>
        /// Flushes the buffers of a specified file and causes all buffered data to be written to a file.
        /// https://docs.microsoft.com/en-us/windows/desktop/api/fileapi/nf-fileapi-flushfilebuffers
        /// Not strictly necessary, but helps predicatability, since the system will flush immediately 
        /// if it can, as opposed to waiting for the operating system to do it when it wants. 
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool FlushFileBuffers(SafeFileHandle handle);

        /// <summary>
        /// Opens an existing file for reading. When the SafeFileHandle is disposed it closes the file.
        /// </summary>
        public static SafeFileHandle OpenForReading(string file)
        {
            var r = CreateFile(file, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (r.IsInvalid) throw new Win32Exception();
            return r;
        }

        /// <summary>
        /// Creates or opens a file to be overwritten. When the SafeFileHandle is disposed it closes the file, 
        /// but does not flush the buffer explicitly. Flushing happens lazily when the
        /// system decides.
        /// </summary>
        public static SafeFileHandle OpenForWriting(string file)
        {
            var r = CreateFile(file, GENERIC_WRITE, 0, IntPtr.Zero, CREATE_ALWAYS, 0, IntPtr.Zero);
            if (r.IsInvalid) throw new Win32Exception();
            return r;
        }

        /// <summary>
        /// Attempts to write n bytes to the stream, returning the number of bytes actually written.
        /// </summary>
        public static int WriteFile(this SafeFileHandle hFile, IntPtr pBuffer, int nBytesToWrite)
        {
            if (hFile.IsInvalid) throw new Exception("Invalid file handle");
            if (!WriteFile(hFile, pBuffer, nBytesToWrite, out var nBytesWritten, IntPtr.Zero))
                throw new Win32Exception();
            return nBytesWritten;
        }

        /// <summary>
        /// Flushes the buffers of a specified file and causes all buffered data to be written to a file.
        /// </summary>
        public static SafeFileHandle Flush(this SafeFileHandle hFile)
        {
            if (hFile.IsInvalid) throw new Exception("Invalid file handle");
            if (!FlushFileBuffers(hFile))
                throw new Win32Exception();
            return hFile;
        }

        public static int ReadFile(this SafeFileHandle hFile, IntPtr pBuffer, int nBytesToRead)
        {
            if (hFile.IsInvalid) throw new Exception("Invalid file handle");
            if (!ReadFile(hFile, pBuffer, nBytesToRead, out var nBytesRead, IntPtr.Zero))
                throw new Win32Exception();
            return nBytesRead;
        }

        /// Rumor has it that reading/writing in 64k blocks have the best performance
        public const int BLOCK_SIZE = 65536;

        /// <summary>
        /// Writes data from the buffer to the file.
        /// </summary>
        public static SafeFileHandle Write(this SafeFileHandle hFile, IntPtr buffer, int nBytes)
        {
            var nRemaining = nBytes;
            while (nRemaining > 0)
            {
                var nBytesToWrite = Math.Min(nRemaining, BLOCK_SIZE);
                var nBytesWritten = WriteFile(hFile, buffer, nBytesToWrite);
                if (nBytesWritten == 0)
                    throw new Exception("No bytes written");
                buffer = IntPtr.Add(buffer, nBytesWritten);
                nRemaining -= nBytesWritten;
            }
            return hFile;
        }

        /// <summary>
        /// Read data from the file to the buffer. Assumes enough space in the buffer. 
        /// </summary>
        public static SafeFileHandle Read(this SafeFileHandle hFile, IntPtr buffer, int nBytes)
        {
            var nRemaining = nBytes;
            while (nRemaining > 0)
            {
                var nBytesToRead = Math.Min(nRemaining, BLOCK_SIZE);
                var nBytesRead = ReadFile(hFile, buffer, nBytesToRead);
                if (nBytesRead == 0)
                    throw new Exception("No bytes read");
                buffer = IntPtr.Add(buffer, nBytesRead);
                nRemaining -= nBytesRead;
            }
            return hFile;
        }

        /// <summary>
        /// Read data from the file to the buffer. Assumes enough space in the buffer. 
        /// </summary>
        public static SafeFileHandle Read(this SafeFileHandle hFile, ByteSpan bytes)
        {
            return Read(hFile, bytes.Ptr, bytes.ByteCount);
        }

        /// <summary>
        /// Writes all data from the buffer to the file.
        /// </summary>
        public static SafeFileHandle Write(this SafeFileHandle hFile, ByteSpan bytes)
        {
            return Write(hFile, bytes.Ptr, bytes.ByteCount);
        }

        /// <summary>
        /// Writes all data from the buffer to the file.
        /// </summary>
        public static void WriteAllBytes(string filePath, ByteSpan bytes)
        {
            using (var hFile = OpenForWriting(filePath))
            {
                hFile.Write(bytes);
                hFile.Flush();
            }
        }

        /// <summary>
        /// Writes all data from the buffer to the file.
        /// </summary>
        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            using (var pin = bytes.Pin())
                WriteAllBytes(filePath, pin.Bytes);
        }

        /// <summary>
        /// Read data from the file into a byte array. 
        /// </summary>
        public static byte[] Read(this SafeFileHandle hFile, int size)
        {
            var r = new byte[size];
            using (var pin = r.Pin())
                hFile.Read(pin.Bytes);
            return r;
        }

        /// <summary>
        /// Reads n bytes from the file into an unmanaged buffer. Remember to dispose the buffer when you are finished with it.
        /// </summary>
        public static UnmanagedBuffer ReadUnmanaged(this SafeFileHandle hFile, int size)
        {
            var r = new UnmanagedBuffer(size);
            hFile.Read(r.Bytes);
            return r;
        }

        /// <summary>
        /// Reads the entire file into an unmanaged buffer. Remember to dispose the buffer when you are finished with it.
        /// </summary>
        public static UnmanagedBuffer ReadAllBytesUnmanaged(string filePath)
        {
            var fi = new FileInfo(filePath);
            using (var hFile = OpenForReading(filePath))
                return hFile.ReadUnmanaged((int)fi.Length);
        }

        /// <summary>
        /// Reads the entire file into a byte array. 
        /// </summary>
        public static byte[] ReadAllBytes(string filePath)
        {
            var fi = new FileInfo(filePath);
            using (var hFile = OpenForReading(filePath))
                return hFile.Read((int)fi.Length);
        }
    }
}
