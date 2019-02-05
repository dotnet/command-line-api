// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
    internal class TypeWithParameterlessInvokeAndCtor
    {
        // included for clarity
        public TypeWithParameterlessInvokeAndCtor()
        { }

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke()
        {
            return Task.FromResult(86);
        }

        public Task<int> SomethingElse()
        {
            return Task.FromResult(87);
        }

    }
}
