using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace ObjectBinder
{
    public static class OptionExtensions
    {
        public static Option<T> Name<T>( this Option<T> option, string name )
        {
            option.Name = name;
            return option;
        }

        public static Option<T> Description<T>(this Option<T> option, string description)
        {
            option.Description = description;
            return option;
        }

        public static Option<T> DefaultValue<T>(this Option<T> option, T defaultValue )
        {
            option.Argument.SetDefaultValue(defaultValue);
            return option;
        }

        public static Option<T> Validator<T>(this Option<T> option, IOptionValidator<T> validator )
        {
            option.AddValidator( x => validator.GetErrorMessage( x.GetValueOrDefault<T>() ) );
            return option;
        }
    }
}