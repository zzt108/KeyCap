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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using KeyCap.Support.IO;
using KeyCap.Support.UI;
using KeyCap.Util;
using Newtonsoft.Json;

namespace KeyCap.Format
{
    // TODO Open Logitech G13 xml export file and convert it to standard keyboard
    // TODO Open keyboard config based on process change
    public class ConfigFileManager
    {
        private class ExternalData
        {
            public ExternalData(List<RemapEntry> listRemapEntries)
            {
                this.RemapEntries = listRemapEntries;
                // foreach (var remapEntry in listRemapEntries)
                // {
                //     this.ListJsonRemapEntries.Add(remapEntry.SerializeToJson());
                // }
            }

            public int FileDataPrefix = ConfigFileManager.FileDataPrefix;

            public int DataFormatVersion = ConfigFileManager.DataFormatVersion;
            // ReSharper disable once MemberCanBePrivate.Local
            public List<RemapEntry> RemapEntries;
        }

        private const int FileDataPrefix = 0x0E0CA000;
        private const int DataFormatVersion = 0x1;

        /// <summary>
        /// Saves the remap entries to a versioned file format
        /// </summary>
        /// <param name="listRemapEntries">The entries to persist</param>
        /// <param name="sFileName">The name of the file to save to</param>
        public static void SaveFile(List<RemapEntry> listRemapEntries, FileGroup zFileGroup)
        {
            using (var fileStream = new FileStream(zFileGroup.GetFilenameWithExtension(ValidExtension.KFG), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                StreamUtil.WriteIntToStream(fileStream, FileDataPrefix);
                StreamUtil.WriteIntToStream(fileStream, DataFormatVersion);
                listRemapEntries.ForEach(zEntry =>
                {
                    var arrayBytes = zEntry.SerializeToBytes();
                    fileStream.Write(arrayBytes, 0, arrayBytes.Length);
                });
                fileStream.Close();
            }
        }

        /// <summary>
        /// Saves the remap entries to a versioned JSON file
        /// </summary>
        /// <param name="listRemapEntries">The entries to persist</param>
        /// <param name="zFileGroup">The name of the file to save to</param>
        public static void SaveFileJson(List<RemapEntry> listRemapEntries, FileGroup zFileGroup)
        {
            // var serializer = new JsonSerializer();
            // serializer.Converters.Add(new JavaScriptDateTimeConverter());
            // serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var sw = new StreamWriter(zFileGroup.GetFilenameWithExtension(ValidExtension.JSON)))
            {
                var json = JsonConvert.SerializeObject(new ExternalData(listRemapEntries), Formatting.Indented);
                sw.Write(json);
            }
        }

        /// <summary>
        /// Loads the remap entries from the specified file
        /// </summary>
        /// <param name="zFileGroup"></param>
        /// <returns></returns>
        public static List<RemapEntry> LoadFile(FileGroup zFileGroup)
        {
            var listConfigs = new List<RemapEntry>();
            try
            {
                // zFileStream = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                FileStream zFileStream;
                var filenameWithExtension = zFileGroup.GetFilenameWithExtension(ValidExtension.KFG);
                using (zFileStream = new FileStream(filenameWithExtension, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var nPrefix = StreamUtil.ReadIntFromStream(zFileStream);
                    CheckFilePrefix(filenameWithExtension, nPrefix);

                    var nDataFormatVersion = StreamUtil.ReadIntFromStream(zFileStream);
                    CheckFileVersion(filenameWithExtension, nDataFormatVersion);

                    while (zFileStream.Position < zFileStream.Length)
                    {
                        listConfigs.Add(new RemapEntry(zFileStream));
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return listConfigs;
        }

        #region Private functions

        private static void CheckFileVersion(string sFileName, int nDataFormatVersion)
        {
            if (nDataFormatVersion != DataFormatVersion)
            {
                throw new Exception("{} indicates an unsupported data format.".FormatString(sFileName));
            }
        }

        private static void CheckFilePrefix(string sFileName, int nPrefix)
        {
            if (nPrefix != FileDataPrefix)
            {
                throw new Exception(
                    "{} does not have the correct data prefix. This is likely an unsupported format."
                        .FormatString(
                            sFileName));
            }
        }

        #endregion

        /// <summary>
        /// Loads the remap entries from the specified file
        /// </summary>
        /// <param name="zFileGroup"></param>
        /// <returns></returns>
        public static List<RemapEntry> LoadFileJson(FileGroup zFileGroup)
        {
            var filenameWithExtension = zFileGroup.GetFilenameWithExtension(ValidExtension.JSON);
            using (var sr = new StreamReader(filenameWithExtension))
            {
                var json = sr.ReadToEnd();
                var data = JsonConvert.DeserializeObject<ExternalData>(json);
                if (data != null)
                {
                    CheckFilePrefix(filenameWithExtension, data.FileDataPrefix);

                    CheckFileVersion(filenameWithExtension, data.DataFormatVersion);
                    return data.RemapEntries;
                }
                else
                {
                    throw new Exception($"{zFileGroup} import is not successfull");
                }
            }
        }
    }
}