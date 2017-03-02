// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionError
    {
        public OptionError(
            string message, 
            string token,
            AppliedOption option = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }

            Message = message;
            Option = option;
            Token = token;
        }

        public string Message { get; }

        public AppliedOption Option { get; }

        public string Token { get;  }

        public override string ToString() => Message;
    }
}