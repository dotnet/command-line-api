// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Binding
{
    internal static class Formatter
    {
        public static string Format(object obj)
        {
            var sb = new StringBuilder();

            if (obj.GetType().IsPrimitive)
            {
                sb.AppendLine($"{obj} ({obj.GetType()})");
            }
            else
            {
                switch (obj)
                {
                    case string s:
                        sb.AppendLine($"{obj} ({obj.GetType()})");
                        break;

                    case FileSystemInfo fsi:
                        sb.AppendLine($"{fsi} ({fsi.GetType()})\nExists? {fsi.Exists}");
                        break;

                    case ParseResult parseResult:
                        if (parseResult.Errors.Any())
                        {
                            foreach (var error in parseResult.Errors)
                            {
                                sb.AppendLine(error.ToString());
                            }
                        }
                        else
                        {
                            sb.AppendLine(parseResult.ToString());
                        }

                        break;

                    default:

                        foreach (var property in obj.GetType()
                                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                    .Where(p => p.CanRead)
                                                    .OrderBy(p => p.Name))
                        {
                            sb.AppendLine($"{property.Name}: {property.GetValue(obj)} ({property.PropertyType})");
                        }

                        break;
                }
            }

            return sb.ToString();
        }
    }
}
