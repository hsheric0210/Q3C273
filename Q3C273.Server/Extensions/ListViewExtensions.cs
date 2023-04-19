using Q3C273.Server.Helper;
using Q3C273.Server.Utilities;
using Q3C273.Shared.Helpers;
using System;
using System.Windows.Forms;

namespace Q3C273.Server.Extensions
{
    public static class ListViewExtensions
    {
        private const uint SET_COLUMN_WIDTH = 4126;
        private static readonly IntPtr AUTOSIZE_USEHEADER = new IntPtr(-2);

        /// <summary>
        /// Automatically determines the correct column size on the the given listview.
        /// </summary>
        /// <param name="targetListView">The listview whose columns are to be autosized.</param>
        public static void AutosizeColumns(this ListView targetListView)
        {
            if (PlatformHelper.RunningOnMono)
                return;
            for (var lngColumn = 0; lngColumn <= targetListView.Columns.Count - 1; lngColumn++)
            {
                NativeMethods.SendMessage(targetListView.Handle, SET_COLUMN_WIDTH, new IntPtr(lngColumn), AUTOSIZE_USEHEADER);
            }
        }

        /// <summary>
        /// Selects all items on the given listview.
        /// </summary>
        /// <param name="targetListView">The listview whose items are to be selected.</param>
        public static void SelectAllItems(this ListView targetListView)
        {
            NativeMethodsHelper.SetItemState(targetListView.Handle, -1, 2, 2);
        }
    }
}