// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace System.CommandLine.Tests.Binding
{
    internal class TypeWithStaticsInvokeAndCtor
    {
        public static int StaticIntProperty { get; set; } = 67;
        public static string StaticStringProperty { get; set; } 
        public int IntValueFromCtor { get; }
        public string StringValueFromCtor { get; }

        public TypeWithStaticsInvokeAndCtor(int intFromCtor, string stringFromCtor)
        {
            IntValueFromCtor = intFromCtor;
            StringValueFromCtor = stringFromCtor;
        }

        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public Task<int> Invoke(string stringParam, int intParam)
        {
            return Task.FromResult(96);
        }

        public Task<int> SomethingElse(int intParam, string stringParam)
        {
            return Task.FromResult(97);
        }
    }
}
