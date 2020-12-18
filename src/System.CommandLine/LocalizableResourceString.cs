// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace System.CommandLine
{
    /// <summary>
    /// A localizable resource string that may possibly be formatted differently depending on culture.
    /// Copied from https://github.com/dotnet/roslyn/blob/cf55f3a58e47298426fa971d3bd9d8857c746c65/src/Compilers/Core/Portable/Diagnostic/LocalizableResourceString.cs
    /// </summary>
    public sealed class LocalizableResourceString : LocalizableString
    {
        private readonly string _nameOfLocalizableResource;
        private readonly ResourceManager _resourceManager;
        private readonly Type _resourceSource;
        private readonly string[] _formatArguments;

        /// <summary>
        /// Creates a localizable resource string with no formatting arguments.
        /// </summary>
        /// <param name="nameOfLocalizableResource">nameof the resource that needs to be localized.</param>
        /// <param name="resourceManager"><see cref="ResourceManager"/> for the calling assembly.</param>
        /// <param name="resourceSource">Type handling assembly's resource management. Typically, this is the static class generated for the resources file from which resources are accessed.</param>
        public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource)
            : this(nameOfLocalizableResource, resourceManager, resourceSource, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Creates a localizable resource string that may possibly be formatted differently depending on culture.
        /// </summary>
        /// <param name="nameOfLocalizableResource">nameof the resource that needs to be localized.</param>
        /// <param name="resourceManager"><see cref="ResourceManager"/> for the calling assembly.</param>
        /// <param name="resourceSource">Type handling assembly's resource management. Typically, this is the static class generated for the resources file from which resources are accessed.</param>
        /// <param name="formatArguments">Optional arguments for formatting the localizable resource string.</param>
        public LocalizableResourceString(string nameOfLocalizableResource, ResourceManager resourceManager, Type resourceSource, params string[] formatArguments)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _nameOfLocalizableResource = nameOfLocalizableResource ?? throw new ArgumentNullException(nameof(nameOfLocalizableResource));
            _resourceSource = resourceSource ?? throw new ArgumentNullException(nameof(resourceSource));
            _formatArguments = formatArguments ?? throw new ArgumentNullException(nameof(formatArguments));
        }

        protected override string GetText(IFormatProvider? formatProvider)
        {
            var culture = formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture;
            var resourceString = _resourceManager.GetString(_nameOfLocalizableResource, culture);
            return resourceString != null ?
                _formatArguments.Length > 0 ? string.Format(resourceString, _formatArguments) : resourceString :
                string.Empty;
        }

        protected override bool AreEqual(object? other)
        {
            var otherResourceString = other as LocalizableResourceString;
            return otherResourceString != null &&
                _nameOfLocalizableResource == otherResourceString._nameOfLocalizableResource &&
                _resourceManager == otherResourceString._resourceManager &&
                _resourceSource == otherResourceString._resourceSource &&
                _formatArguments.SequenceEqual(otherResourceString._formatArguments);
        }

        protected override int GetHash()
        {
            int hashCode = 
                (
                    _nameOfLocalizableResource.GetHashCode(),
                    _resourceManager.GetHashCode(), 
                    _resourceSource.GetHashCode()
                ).GetHashCode();
            foreach(var formatArgument in _formatArguments)
            {
                hashCode = (hashCode, formatArgument).GetHashCode();
            }
            return hashCode;
        }
    }
}