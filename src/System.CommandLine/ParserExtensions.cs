// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public static class ParserExtensions
    {
        public static ParseResult Parse(
            this Parser parser,
            string input) =>
            parser.Parse(input.Tokenize().ToArray(), input);

        public static T CreateInstance<T>(
            this Parser parser,
            string commandLine)
        {
            var parseResult = parser.Parse(commandLine);

            var bindingContext = new BindingContext(parseResult, parser);

            var binder = new ReflectionBinder(typeof(T));

            var instance = (T)binder.GetTarget(bindingContext);

            return instance;
        }

        public static void UpdateInstance<T>(
            this Parser parser,
            T instance,
            string commandLine)
        {
            var parseResult = parser.Parse(commandLine);

            var bindingContext = new BindingContext(parseResult, parser);

            var binder = new ReflectionBinder(typeof(T));


        }
    }


}
