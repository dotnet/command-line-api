using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JackFruit
{
    internal class SlnActions
    {
        public static async Task<int> AddAsync(FileInfo slnFile, FileInfo projectFile)
        {
            Console.WriteLine(
            $@"Sln/Add(
        Sln File: {slnFile}
        Project File: {projectFile}
    )");
            return await Task.FromResult(0);
        }

        public static async Task<int> ListAsync(FileInfo slnFile)
        {
            Console.WriteLine(
            $@"Sln/List(
        Sln File: {slnFile}
    )");
            return await Task.FromResult(0);
        }

        public static async Task<int> RemoveAsync(FileInfo slnFile, FileInfo projectFile)
        {
            Console.WriteLine(
            $@"Sln/Remove(
        Sln File: {slnFile}
        Project File: {projectFile}
    )");
            return await Task.FromResult(0);
        }
    }
}
