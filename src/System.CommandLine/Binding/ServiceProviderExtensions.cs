// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Binding;

internal static class ServiceProviderExtensions
{
    public static T GetService<T>(this IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetService(typeof(T));

        if (service is null)
        {
            throw new ArgumentException($"Service not found for type {typeof(T)}");
        }

        return (T)service;
    }
}