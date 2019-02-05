// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
    internal class TypeWithInvokeNoCtor
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(66);
        }

        public Task<int> SomethingElse(int intParam, string stringParam)
        {
            return Task.FromResult(67);
        }
    }
}
