using Q3C273.Server.Helper;
using Q3C273.Server.Utilities;
using Q3C273.Shared.Helpers;
using System;
using System.Windows.Forms;

namespace Q3C273.Server.Controls
{
    internal class AeroListView : ListView
    {
        private const uint WM_CHANGEUISTATE = 0x127;

        private const short UIS_SET = 1;
        private const short UISF_HIDEFOCUS = 0x1;
        private readonly IntPtr _removeDots = new IntPtr(NativeMethodsHelper.MakeWin32Long(UIS_SET, UISF_HIDEFOCUS));

        public ListViewColumnSorter LvwColumnSorter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AeroListView"/> class.
        /// </summary>
        public AeroListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            LvwColumnSorter = new ListViewColumnSorter();
            ListViewItemSorter = LvwColumnSorter;
            View = View.Details;
            FullRowSelect = true;
        }

        /// <summary>
        /// Raises the <see cref="E:HandleCreated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (PlatformHelper.RunningOnMono)
                return;

            if (PlatformHelper.VistaOrHigher)
                // set window theme to explorer
                NativeMethods.SetWindowTheme(Handle, "explorer", null);

            if (PlatformHelper.XpOrHigher)
                // removes the ugly dotted line around focused item
                NativeMethods.SendMessage(Handle, WM_CHANGEUISTATE, _removeDots, IntPtr.Zero);
        }

        /// <summary>
        /// Raises the <see cref="E:ColumnClick" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ColumnClickEventArgs"/> instance containing the event data.</param>
        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == LvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                LvwColumnSorter.Order = LvwColumnSorter.Order == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                LvwColumnSorter.SortColumn = e.Column;
                LvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            if (!VirtualMode)
                Sort();
        }
    }
}