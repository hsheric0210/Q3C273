using Q3C273.Shared.Enums;

namespace Q3C273.Server.Models
{
    public class FileManagerListTag
    {
        public FileType Type { get; set; }

        public long FileSize { get; set; }

        public FileManagerListTag(FileType type, long fileSize)
        {
            Type = type;
            FileSize = fileSize;
        }
    }
}
