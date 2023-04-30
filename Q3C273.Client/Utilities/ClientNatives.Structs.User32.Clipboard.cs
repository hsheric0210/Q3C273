// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/dataxchg/standard-clipboard-formats
        /// </summary>
        public enum StandardClipboardFormats : uint
        {
            CF_TEXT = 1,
            CF_OEMTEXT = 7,
            CF_UNICODETEXT = 13,
            CF_HDROP = 15,
            CF_DSPTEXT = 0x81,
        }
    }
}