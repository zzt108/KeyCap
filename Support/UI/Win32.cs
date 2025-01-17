////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KeyCap.Support.UI
{
	/// <summary>
	/// This class allows for access to win32 functionality that is not directly supported in .NET (hiding/showing the console)
	/// </summary>
	public class Win32 
	{
        // Win32 #defines
        private const int SwHide = 0;
        private const int SwRestore = 9;
        private const int WmSetredraw = 0xB;
        private const int SwShownoactivate = 4;
        private const int SwShow = 5;
        private const uint SwpNoactivate = 0x0010;

		private Win32(){}

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleTitle(
			string sConsoleTitle // window title
			);

		[DllImport("user32")] 
		private static extern int FindWindow( 
			string lpClassName, // class name 
			string lpWindowName // window name 
			); 

		[DllImport("user32")]
		private static extern int ShowWindow(
			int hwnd,		// window handle
			int nCmdShow	// command
			);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,           // window handle
             int hWndInsertAfter,    // placement-order handle
             int x,          // horizontal position
             int y,          // vertical position
             int cx,         // width
             int cy,         // height
             uint uFlags);       // window positioning flags

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

		/// <summary>
		/// Used to show/hide windows by their title.
		/// </summary>
		/// <param name="sName"></param>
		/// <param name="bShow"></param>
		private static void ShowWindow(string sName, bool bShow)
		{
			var nHandle = FindWindow(null, sName);

			if(bShow)
			{
				ShowWindow(nHandle, SwRestore);
			}
			else
			{
				ShowWindow(nHandle, SwHide);
			}
		}

		/// <summary>
		/// Used to show/hide the console window in console enabled applications. The Console will flicker on at startup.
		/// </summary>
        /// <param name="sConsoleTitle">A unique name for this instance of the console</param>
		/// <param name="bShow">Flag indicating whether to show or hide the console</param>
		public static void ShowConsole(string sConsoleTitle, bool bShow)
		{
			// force the name of console and change the visibility
			SetConsoleTitle(sConsoleTitle);
            ShowWindow(sConsoleTitle, bShow);
		}

        public static void SetRedraw(IntPtr handle, bool bRedraw)
        {
            SendMessage(handle, WmSetredraw, (IntPtr)(bRedraw ? 1 : 0), IntPtr.Zero);
        }

        public static void ShowTopmost(IntPtr nFormHandle)
        {
            ShowWindow(nFormHandle.ToInt32(), SwShow);
            SetForegroundWindow(nFormHandle.ToInt32());
        }

        public static void ShowInactiveTopmost(IntPtr nFormHandle, int nLeft, int nTop, int nWidth, int nHeight)
        {
            ShowWindow(nFormHandle.ToInt32(), SwShownoactivate);
            SetWindowPos(nFormHandle.ToInt32(), -1, nLeft, nTop, nWidth, nHeight, SwpNoactivate);
        }


        #region RichTextBox

        // these are specific to rich text boxes
        private const int EmGetscrollpos = 0x0400 + 221;
        private const int EmSetscrollpos = 0x0400 + 222;

        // it's this or using actual pointers and forcing unsafe code
        [DllImport("user32.dll")]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, ref Point lp);

        public static Point GetRichTextScrollPosition(IntPtr handle)
        {
            var pLocation = new Point();
            SendMessage(handle, EmGetscrollpos, (IntPtr)0, ref pLocation);
            return pLocation;
        }

        public static void SetRichTextScrollPosition(IntPtr handle, Point pLocation)
        {
            SendMessage(handle, EmSetscrollpos, (IntPtr)0, ref pLocation);
        }

        #endregion

    } 
}