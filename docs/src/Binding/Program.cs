// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Binding
{
    static class Program
    {
        static async Task<int> Main(
            string session = null,
            string region = null,
            string project = null,
            string package = null,
            string[] args = null)
        {
            return region switch
            {
                "GetValueFromOptionArgument" =>
                    GetValueSample.GetValueFromOptionArgument(),
                    
                "ComplexTypes" =>
                    await HandlerBindingSample.ComplexTypes(),

                "FileSystemTypes" =>
                    await HandlerBindingSample.FileSystemTypes(),
                    
                "MultipleArgs" =>
                    await HandlerBindingSample.MultipleArgs(),
                    
                "Bool" =>
                    await HandlerBindingSample.Bool(),
                    
                "Enum" =>
                    await HandlerBindingSample.Enum(),
                    
                "Enumerables" =>
                    await HandlerBindingSample.Enumerables(),
                    
                "DependencyInjection" =>
                    await HandlerBindingSample.DependencyInjection(),
                    
                _ => 
                    throw new ArgumentException($"There's no case in Program.Main for {nameof(region)} '{region}'")
            };
        }
    }
}
