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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using KeyCap.Support.IO;

namespace KeyCap.Support.UI
{
    /// <summary>
    /// Handles Form "dirty/clean" states in relation to file loading / saving. Acts as
    /// an abstract class, but is not. (forms & abstraction break the IDE in MSVS2003)
    /// Be sure to override SaveFormData and OpenFormData
    /// </summary>
    //public abstract class AbstractDirtyForm
    public class AbstractDirtyForm : Form
    {
        private string m_sCurrentDirectory = string.Empty;
        protected string SBaseTitle = string.Empty;
        protected FileGroup zLoadedFile;
        protected string SFileOpenFilter = string.Empty;
        protected string SFileSaveFilter = string.Empty;

        public FileGroup LoadedFile
        {
            get { return zLoadedFile; }
        }

        protected bool Dirty { get; private set; }

        /// <summary>
        /// This method should have an override that performs the save of the data to the file.
        /// </summary>
        /// <param name="zFileGroup"></param>
        /// <returns>true on success, false otherwise</returns>
        protected virtual bool SaveFormData(FileGroup zFileGroup)
        {
            MessageBox.Show(this, "DEV Error: Please override AbstractDirtyForm.SaveFormData");
            return false;
        }

        /// <summary>
        /// This method should have an override that performs the load of the data from the file.
        /// </summary>
        /// <param name="zFileGroup"></param>
        /// <returns>true on success, false otherwise</returns>
        protected virtual bool OpenFormData(FileGroup zFileGroup)
        {
            MessageBox.Show(this, "DEV Error: Please override AbstractDirtyForm.OpenFormData");
            return false;
        }

        /// <summary>
        /// Gets the current directory associated with the form
        /// </summary>
        /// <returns>Current directory for the form, defaults to the current environment</returns>
        private string GetDialogDirectory()
        {
            if (Directory.Exists(m_sCurrentDirectory))
                return m_sCurrentDirectory;
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// Marks this form as dirty (needing save)
        /// </summary>
        public void MarkDirty()
        {
            if (!Dirty)
            {
                if (!Text.EndsWith("*"))
                    Text += " *";

                Dirty = true;
            }
        }

        /// <summary>
        /// Marks this form as clean (save not needed)
        /// </summary>
        public void MarkClean()
        {
            if (Dirty)
            {
                Text = Text.Replace("*", "").Trim();
                Dirty = false;
            }
        }

        /// <summary>
        /// Initializes a simple new file
        /// </summary>
        protected void InitNew()
        {
            zLoadedFile = null;
            Text = SBaseTitle;
            MarkClean();
        }

        /// <summary>
        /// Initializes the Open process via the OpenFileDialog
        /// </summary>
        protected void InitOpen()
        {
            var ofn = new OpenFileDialog
            {
                InitialDirectory = GetDialogDirectory(),
                Filter = 0 == SFileOpenFilter.Length
                    ? "All files (*.*)|*.*"
                    : SFileOpenFilter
            };
            if (DialogResult.OK == ofn.ShowDialog(this))
            {
                FileGroup fg = new FileGroup(ofn.FileName);
                var sPath = Path.GetDirectoryName(ofn.FileName);
                if (null != sPath)
                {
                    if (Directory.Exists(sPath))
                    {
                        m_sCurrentDirectory = sPath;
                        if (OpenFormData(fg))
                        {
                            SetLoadedFile(fg);
                            return;
                        }
                    }
                }

                MessageBox.Show(this, $"Error opening [{ofn.FileName}] Wrong file type?", "File Open Error");
            }
        }

        /// <summary>
        /// Initializes the open process with the file specified.
        /// </summary>
        /// <param name="sFileName">The file to open the data from</param>
        /// <returns>true on success, false otherwise</returns>
        protected bool InitOpen(string sFileName)
        {
            var fg = new FileGroup(sFileName);
            if (OpenFormData(fg))
            {
                SetLoadedFile(fg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the currently loaded file and marks the state as clean
        /// </summary>
        /// <param name="sFileName"></param>
        protected void SetLoadedFile(FileGroup fg)
        {
            zLoadedFile = fg;
            Text = $"{SBaseTitle} [{zLoadedFile.FileName}]";
            MarkClean();
        }

        /// <summary>
        /// Event to associate with the OnClose event of the form
        /// </summary>
        /// <param name="eArg"></param>
        protected void SaveOnClose(CancelEventArgs eArg)
        {
            SaveOnEvent(eArg, true);
        }

        protected void SaveOnEvent(CancelEventArgs eArg, bool bAllowCancel)
        {
            if (Dirty)
            {
                switch (MessageBox.Show(this, "Would you like to save any changes?",
                    "Save",
                    (bAllowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo),
                    MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        InitSave(false);
                        break;
                    case DialogResult.No:
                        MarkClean();
                        break;
                    case DialogResult.Cancel:
                        eArg.Cancel = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes the Save / Save As dialog
        /// </summary>
        /// <param name="bForceSaveAs"></param>
        protected void InitSave(bool bForceSaveAs)
        {
            if (zLoadedFile is null || bForceSaveAs)
            {
                var sfn = new SaveFileDialog
                {
                    InitialDirectory = GetDialogDirectory(),
                    OverwritePrompt = true
                };
                if (0 == SFileSaveFilter.Length)
                    sfn.Filter = "All files (*.*)|*.*";
                else
                    sfn.Filter = SFileSaveFilter;

                if (DialogResult.OK == sfn.ShowDialog(this))
                {
                    var sPath = Path.GetDirectoryName(sfn.FileName);
                    if (null != sPath)
                    {
                        if (Directory.Exists(sPath))
                        {
                            m_sCurrentDirectory = sPath;
                            SetLoadedFile(new FileGroup(sfn.FileName));
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            if (!SaveFormData(zLoadedFile))
            {
                MessageBox.Show(this, $"Error saving to file: {zLoadedFile.FileName}", "File Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MarkClean();
            }
        }
    }
}