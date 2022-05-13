using System.IO;

namespace KeyCap.Support.IO
{
    /// <summary>
    /// Representation of the same key map in different extensions
    /// </summary>
    public class FileGroup
    {
        public static string FileOpenFilter { get; private set; }
        public static string FileSaveFilter { get; private set; }
        public string FileName { get; }
        public string Extension { get; set; }

        public FileGroup(string fileName)
        {
            FileName = fileName;
            Extension = Path.GetExtension(fileName);
        }

        public string GetFilenameWithExtension(string extension)
        {
            return Path.ChangeExtension(FileName, extension);
        }
        
        public static void SetFilterNames(string sProductName)
        {
            FileOpenFilter =
                $"{sProductName} Json files (*{ValidExtension.JSON})|*{ValidExtension.JSON}|{sProductName} Config files (*{ValidExtension.KFG})|*{ValidExtension.KFG}|All files (*.*)|*.*";
            FileSaveFilter =
                $"{sProductName} Json files (*{ValidExtension.JSON})|*{ValidExtension.JSON}|{sProductName} Config files (*{ValidExtension.KFG})|*{ValidExtension.KFG}|All files (*.*)|*.*";
        }
    }

    public static class ValidExtension
    {
        public const string JSON = ".json";
        public const string KFG = ".kfg";
    }
}