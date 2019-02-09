// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
    internal class TypeWithInvokeAndCtor
    {
        public int IntValueFromCtor { get; }
        public string StringValueFromCtor { get; }

        public TypeWithInvokeAndCtor(int intFromCtor, string stringFromCtor)
        {
            IntValueFromCtor = intFromCtor;
            StringValueFromCtor = stringFromCtor;
        }

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(76);
        }
    }
}
