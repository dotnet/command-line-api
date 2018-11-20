using System.IO;

namespace JackFruit
{
    internal class Remove : DotnetJackFruit
    {
        public FileInfo ProjectFileArg { get; set; }

        internal class Package : Remove
        {
            public string PackageNameArg { get; set; }
        }

        internal class Reference : Remove
        {
            public FileInfo ProjectPathArg { get; set; }

            public string Framework { get; set; }
        }
    }
}
