// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    /// <summary>
    /// A string that may possibly be formatted differently depending on culture.
    /// NOTE: Types implementing <see cref="LocalizableString"/> must be serializable.
    /// </summary>
    public abstract class LocalizableString : IFormattable, IEquatable<LocalizableString?>
    {
        /// <summary>
        /// Fired when an exception is raised by any of the public methods of <see cref="LocalizableString"/>.
        /// If the exception handler itself throws an exception, that exception is ignored.
        /// </summary>
        public event EventHandler<Exception>? OnException;

        /// <summary>
        /// Formats the value of the current instance using the optionally specified format. 
        /// </summary>
        public string ToString(IFormatProvider? formatProvider)
        {
            try
            {
                return GetText(formatProvider);
            }
            catch (Exception ex)
            {
                RaiseOnException(ex);
                return string.Empty;
            }
        }

        public static explicit operator string?(LocalizableString localizableResource) 
            => localizableResource?.ToString(null);

        public static implicit operator LocalizableString(string? fixedResource) 
            => FixedLocalizableString.Create(fixedResource);

        public sealed override string ToString() 
            => ToString(null);

        string IFormattable.ToString(string? ignored, IFormatProvider? formatProvider) 
            => ToString(formatProvider);

        public sealed override int GetHashCode()
        {
            try
            {
                return GetHash();
            }
            catch (Exception ex)
            {
                RaiseOnException(ex);
                return 0;
            }
        }

        public sealed override bool Equals(object? other)
        {
            try
            {
                return AreEqual(other);
            }
            catch (Exception ex)
            {
                RaiseOnException(ex);
                return false;
            }
        }

        public bool Equals(LocalizableString? other) 
            => Equals((object?)other);

        /// <summary>
        /// Formats the value of the current instance using the optionally specified format.
        /// Provides the implementation of ToString. ToString will provide a default value
        /// if this method throws an exception.
        /// </summary>
        protected abstract string GetText(IFormatProvider? formatProvider);

        /// <summary>
        /// Provides the implementation of GetHashCode. GetHashCode will provide a default value
        /// if this method throws an exception.
        /// </summary>
        /// <returns></returns>
        protected abstract int GetHash();

        /// <summary>
        /// Provides the implementation of Equals. Equals will provide a default value
        /// if this method throws an exception.
        /// </summary>
        /// <returns></returns>
        protected abstract bool AreEqual(object? other);

        private void RaiseOnException(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return;
            }

            try
            {
                OnException?.Invoke(this, ex);
            }
            catch
            {
                // Ignore exceptions from the exception handlers themselves.
            }
        }

        private sealed class FixedLocalizableString : LocalizableString
        {
            /// <summary>
            /// FixedLocalizableString representing an empty string.
            /// </summary>
            private static readonly FixedLocalizableString _empty = new FixedLocalizableString(string.Empty);

            private readonly string _fixedString;

            public static FixedLocalizableString Create(string? fixedResource)
            {
                if (string.IsNullOrEmpty(fixedResource))
                {
                    return _empty;
                }

                return new FixedLocalizableString(fixedResource!);
            }

            private FixedLocalizableString(string fixedResource)
                => _fixedString = fixedResource;

            protected override string GetText(IFormatProvider? formatProvider)
                => _fixedString;

            protected override bool AreEqual(object? other)
            {
                var fixedStr = other as FixedLocalizableString;
                return fixedStr != null && string.Equals(_fixedString, fixedStr._fixedString);
            }

            protected override int GetHash()
                => _fixedString?.GetHashCode() ?? 0;
        }
    }
}