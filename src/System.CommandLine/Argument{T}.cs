// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class Argument<T> : Argument
    {
        public Argument(ConvertArgument convert = null)
        {
            Arity = ArgumentArity.DefaultForType(typeof(T));

            ArgumentType = typeof(T);

            if (convert != null)
            {
                ConvertArguments = convert;
            }
        }
    }
}
