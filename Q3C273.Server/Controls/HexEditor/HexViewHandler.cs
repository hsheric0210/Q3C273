using System;
using System.Drawing;
using System.Windows.Forms;

namespace Q3C273.Server.Controls.HexEditor
{
    public class HexViewHandler
    {
        #region Fields

        bool _isEditing;

        /// <summary>
        /// Contains info about how to 
        /// present the hex values 
        /// (Upper or Lower case)
        /// </summary>
        string _hexType = "X2";

        /// <summary>
        /// Contains the boundary for one single
        /// hexa value that is visible
        /// </summary>
        Rectangle _recHexValue;

        /// <summary>
        /// Contains the format of the hexadecimal
        /// strings that are presented
        /// </summary>
        StringFormat _stringFormat;

        private HexEditor _editor;

        #endregion

        #region Properties

        public int MaxWidth
        {
            get { return _recHexValue.X + _recHexValue.Width * _editor.BytesPerLine; }
        }

        #endregion

        #region Constructor

        public HexViewHandler(HexEditor editor)
        {
            _editor = editor;

            //Set String format for the hex values
            _stringFormat = new StringFormat(StringFormat.GenericTypographic);
            _stringFormat.Alignment = StringAlignment.Center;
            _stringFormat.LineAlignment = StringAlignment.Center;
        }

        #endregion

        #region Method

        #region KeyMouseEvents

        #region KeyEvents

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (IsHex(e.KeyChar))
                HandleUserInput(e.KeyChar);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                if (_editor.SelectionLength > 0)
                {
                    //Remove the selected bytes
                    HandleUserRemove();
                    var index = _editor.CaretIndex;
                    var newLocation = GetCaretLocation(index);
                    _editor.SetCaretStart(index, newLocation);
                }
                else if (_editor.CaretIndex < _editor.LastVisibleByte && e.KeyCode == Keys.Delete)
                {
                    //Remove the byte after the caret
                    _editor.RemoveByteAt(_editor.CaretIndex);
                    var newLocation = GetCaretLocation(_editor.CaretIndex);
                    _editor.SetCaretStart(_editor.CaretIndex, newLocation);
                }
                else if (_editor.CaretIndex > 0 && e.KeyCode == Keys.Back)
                {
                    //Remove byte before the caret
                    var index = _editor.CaretIndex - 1;
                    if (_isEditing)
                        //Remove the byte that is being edited
                        index = _editor.CaretIndex;
                    _editor.RemoveByteAt(index);
                    var newLocation = GetCaretLocation(index);
                    _editor.SetCaretStart(index, newLocation);
                }
                _isEditing = false;
            }
            else if (e.KeyCode == Keys.Up && _editor.CaretIndex - _editor.BytesPerLine >= 0)
            {
                var index = _editor.CaretIndex - _editor.BytesPerLine;

                //Check ig caret is att the end of the line
                if (index % _editor.BytesPerLine == 0 && _editor.CaretPosX >= _recHexValue.X + _recHexValue.Width * _editor.BytesPerLine)
                {
                    var position = new Point(_editor.CaretPosX, _editor.CaretPosY - _recHexValue.Height);

                    //check that this is not the last row (nothing above)
                    if (index == 0)
                    {
                        //Last row do not change index and position
                        position = new Point(_editor.CaretPosX, _editor.CaretPosY);
                        index = _editor.BytesPerLine;
                    }

                    if (e.Shift)
                        _editor.SetCaretEnd(index, position);
                    else
                        _editor.SetCaretStart(index, position);
                    _isEditing = false;
                }
                else
                {
                    HandleArrowKeys(index, e.Shift);
                }
            }
            else if (e.KeyCode == Keys.Down && (_editor.CaretIndex - 1) / _editor.BytesPerLine < _editor.HexTableLength / _editor.BytesPerLine)
            {
                var index = _editor.CaretIndex + _editor.BytesPerLine;

                if (index > _editor.HexTableLength)
                {
                    index = _editor.HexTableLength;
                    HandleArrowKeys(index, e.Shift);
                }
                else
                {
                    var position = new Point(_editor.CaretPosX, _editor.CaretPosY + _recHexValue.Height);

                    if (e.Shift)
                        _editor.SetCaretEnd(index, position);
                    else
                        _editor.SetCaretStart(index, position);
                    _isEditing = false;
                }
            }
            else if (e.KeyCode == Keys.Left && _editor.CaretIndex - 1 >= 0)
            {
                var index = _editor.CaretIndex - 1;
                HandleArrowKeys(index, e.Shift);
            }
            else if (e.KeyCode == Keys.Right && _editor.CaretIndex + 1 <= _editor.HexTableLength)
            {
                var index = _editor.CaretIndex + 1;
                HandleArrowKeys(index, e.Shift);
            }
        }

        public void HandleArrowKeys(int index, bool isShiftDown)
        {
            var position = GetCaretLocation(index);

            if (isShiftDown)
                _editor.SetCaretEnd(index, position);
            else
                _editor.SetCaretStart(index, position);
            _isEditing = false;
        }

        #endregion

        #region MouseEvent

        public void OnMouseDown(int x, int y)
        {
            var iX = (x - _recHexValue.X) / _recHexValue.Width;
            var iY = (y - _recHexValue.Y) / _recHexValue.Height;

            //Check that values are good
            iX = iX > _editor.BytesPerLine ? _editor.BytesPerLine : iX;
            iX = iX < 0 ? 0 : iX;
            iY = iY > _editor.MaxBytesV ? _editor.MaxBytesV : iY;
            iY = iY < 0 ? 0 : iY;

            //Make sure values are withing the given bounds
            if ((_editor.LastVisibleByte - _editor.FirstVisibleByte) / _editor.BytesPerLine <= iY)
            {
                //Check that column is not greater than max
                if ((_editor.LastVisibleByte - _editor.FirstVisibleByte) % _editor.BytesPerLine <= iX)
                    iX = (_editor.LastVisibleByte - _editor.FirstVisibleByte) % _editor.BytesPerLine;
                iY = (_editor.LastVisibleByte - _editor.FirstVisibleByte) / _editor.BytesPerLine;
            }

            //Get the smallest possible location (do not want to exceed the max)
            var index = Math.Min(_editor.LastVisibleByte, _editor.FirstVisibleByte + iX + iY * _editor.BytesPerLine);

            var xPos = iX * _recHexValue.Width + _recHexValue.X;
            var yPos = iY * _recHexValue.Height + _recHexValue.Y;

            _editor.SetCaretStart(index, new Point(xPos, yPos));
            _isEditing = false;
        }

        public void OnMouseDragged(int x, int y)
        {
            var iX = (x - _recHexValue.X) / _recHexValue.Width;
            var iY = (y - _recHexValue.Y) / _recHexValue.Height;

            //Check that values are good
            iX = iX > _editor.BytesPerLine ? _editor.BytesPerLine : iX;
            iX = iX < 0 ? 0 : iX;
            iY = iY > _editor.MaxBytesV ? _editor.MaxBytesV : iY;

            if (_editor.FirstVisibleByte > 0)
                iY = iY < 0 ? -1 : iY;
            else
            {
                iY = iY < 0 ? 0 : iY;
            }

            //Make sure values are withing the given bounds
            if ((_editor.LastVisibleByte - _editor.FirstVisibleByte) / _editor.BytesPerLine <= iY)
            {
                //Check that column is not greater than max
                if ((_editor.LastVisibleByte - _editor.FirstVisibleByte) % _editor.BytesPerLine <= iX)
                    iX = (_editor.LastVisibleByte - _editor.FirstVisibleByte) % _editor.BytesPerLine;
                iY = (_editor.LastVisibleByte - _editor.FirstVisibleByte) / _editor.BytesPerLine;
            }

            //Get the smallest possible location (do not want to exceed the max)
            var index = Math.Min(_editor.LastVisibleByte, _editor.FirstVisibleByte + iX + iY * _editor.BytesPerLine);

            var xPos = iX * _recHexValue.Width + _recHexValue.X;
            var yPos = iY * _recHexValue.Height + _recHexValue.Y;

            _editor.SetCaretEnd(index, new Point(xPos, yPos));
        }

        public void OnMouseDoubleClick()
        {
            if (_editor.CaretIndex < _editor.LastVisibleByte)
            {
                var index = _editor.CaretIndex + 1;
                var newLocation = GetCaretLocation(index);
                _editor.SetCaretEnd(index, newLocation);
            }
        }

        #endregion

        #endregion

        #region PaintMethod

        public void Update(int startPositionX, Rectangle area)
        {
            _recHexValue = new Rectangle(
                startPositionX,
                area.Y,
                (int)(_editor.CharSize.Width * 3),
                (int)_editor.CharSize.Height - 2
            );

            _recHexValue.X += _editor.EntityMargin;
        }

        public void Paint(Graphics g, int index, int startIndex)
        {
            var columnAndRow = GetByteColumnAndRow(index);

            if (_editor.IsSelected(index + startIndex))
                PaintByteAsSelected(g, columnAndRow, index + startIndex);
            else
            {
                PaintByte(g, columnAndRow, index + startIndex);
            }
        }

        private void PaintByteAsSelected(Graphics g, Point point, int index)
        {
            var backBrush = new SolidBrush(_editor.SelectionBackColor);
            var textBrush = new SolidBrush(_editor.SelectionForeColor);
            var drawSurface = GetBound(point);
            var hexValue = _editor.GetByte(index).ToString(_hexType);

            g.FillRectangle(backBrush, drawSurface);
            g.DrawString(hexValue, _editor.Font, textBrush, drawSurface, _stringFormat);
        }

        private void PaintByte(Graphics g, Point point, int index)
        {
            var brush = new SolidBrush(_editor.ForeColor);
            var drawSurface = GetBound(point);
            var hexValue = _editor.GetByte(index).ToString(_hexType);

            g.DrawString(hexValue, _editor.Font, brush, drawSurface, _stringFormat);
        }

        #endregion

        public void SetLowerCase()
        {
            _hexType = "x2";
        }

        public void SetUpperCase()
        {
            _hexType = "X2";
        }

        public void Focus()
        {
            var index = _editor.CaretIndex;
            var location = GetCaretLocation(index);
            _editor.SetCaretStart(index, location);
        }

        #endregion

        #region Caret

        /// <summary>
        /// Get the caret current location
        /// in the given bound.
        /// </summary>
        private Point GetCaretLocation(int index)
        {
            var xPos = _recHexValue.X + _recHexValue.Width * (index % _editor.BytesPerLine);
            var yPos = _recHexValue.Y + _recHexValue.Height * ((index - (_editor.FirstVisibleByte + index % _editor.BytesPerLine)) / _editor.BytesPerLine);

            var ret = new Point(xPos, yPos);
            return ret;
        }

        #endregion

        #region Misc

        private void HandleUserRemove()
        {
            //Calculate where to position the caret after the removal
            var index = _editor.SelectionStart;
            var position = GetCaretLocation(index);
            //Remove all of the selected bytes
            _editor.RemoveSelectedBytes();

            //Set the new position of the caret
            _editor.SetCaretStart(index, position);
        }

        private void HandleUserInput(char key)
        {
            if (!_editor.CaretFocused)
                return;

            //Perform overwrite
            HandleUserRemove();

            if (_isEditing)
            {
                //Editing has already started, should change the second nibble
                _isEditing = false;
                //Load old bytes to allow change
                var oldByte = _editor.GetByte(_editor.CaretIndex);
                //Append the new nibble
                oldByte += Convert.ToByte(key.ToString(), 16);
                _editor.SetByte(_editor.CaretIndex, oldByte);
                //Relocate the caret
                var index = _editor.CaretIndex + 1;
                var newLocation = GetCaretLocation(index);
                _editor.SetCaretStart(index, newLocation);

            }
            else
            {
                //Begin new edit phase
                _isEditing = true;
                var hexByte = key.ToString() + "0";
                var newByte = Convert.ToByte(hexByte, 16);

                if (_editor.HexTable.Length <= 0)
                    _editor.AppendByte(newByte);
                else
                {
                    _editor.InsertByte(_editor.CaretIndex, newByte);
                }

                //Relocate the caret to the middle of the hex value (provide illusion of editing the second value)
                var xPos = _recHexValue.X + _recHexValue.Width * (_editor.CaretIndex % _editor.BytesPerLine) + _recHexValue.Width / 2;
                var yPos = _recHexValue.Y + _recHexValue.Height * ((_editor.CaretIndex - (_editor.FirstVisibleByte + _editor.CaretIndex % _editor.BytesPerLine)) / _editor.BytesPerLine);

                _editor.SetCaretStart(_editor.CaretIndex, new Point(xPos, yPos));
            }
        }

        private Point GetByteColumnAndRow(int index)
        {
            var column = index % _editor.BytesPerLine;
            var row = index / _editor.BytesPerLine;

            var ret = new Point(column, row);
            return ret;
        }

        private RectangleF GetBound(Point point)
        {
            var ret = new RectangleF(
                _recHexValue.X + point.X * _recHexValue.Width,
                _recHexValue.Y + point.Y * _recHexValue.Height,
                _recHexValue.Width,
                _recHexValue.Height
                );

            return ret;
        }

        private bool IsHex(char c)
        {
            return c >= 'a' && c <= 'f' ||
                    c >= 'A' && c <= 'F' ||
                        char.IsDigit(c);
        }

        #endregion
    }
}
