using System;
using System.Text;

namespace ObjectBinder
{
    public class OptionNotInRange<T> : IOptionValidator<T>
        where T : IComparable<T>
    {
        public static OptionNotInRange<TProp> GreaterThan<TProp>( TProp min ) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, MinimumSet = true};

        public static OptionNotInRange<TProp> GreaterThanEqual<TProp>(TProp min) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, IncludeMinimumEqual = true, MinimumSet = true};

        public static OptionNotInRange<TProp> LessThan<TProp>(TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Maximum = max, MaximumSet = true};

        public static OptionNotInRange<TProp> LessThanEqual<TProp>(TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Maximum = max, IncludeMaximumEqual = true, MaximumSet = true};

        public static OptionNotInRange<TProp> GreaterLessThan<TProp>(TProp min, TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, Maximum = max, MinimumSet = true, MaximumSet = true };

        public static OptionNotInRange<TProp> GreaterLessThanEqual<TProp>(TProp min, TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, IncludeMinimumEqual = true, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true, MaximumSet = true };

        public static OptionNotInRange<TProp> GreaterEqualLessThan<TProp>(TProp min, TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, IncludeMinimumEqual = true, Maximum = max, MinimumSet = true, MaximumSet = true };

        public static OptionNotInRange<TProp> GreaterLessEqualThan<TProp>(TProp min, TProp max) where TProp : IComparable<TProp> =>
            new OptionNotInRange<TProp> { Minimum = min, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true, MaximumSet = true };

        protected OptionNotInRange()
        {
        }

        public T Minimum { get; private set; }
        public bool IncludeMinimumEqual { get; private set; }
        protected bool MinimumSet { get; set; }

        public T Maximum { get; private set; }
        public bool IncludeMaximumEqual { get; private set; }
        protected bool MaximumSet { get; set; }

        public bool IsValid( T toCheck )
        {
            if( MinimumSet )
            {
                var comparison = Minimum.CompareTo( toCheck );

                if( comparison < 0 ) return true;
                if( IncludeMinimumEqual && comparison == 0 ) return true;
            }

            if( MaximumSet )
            {
                var comparison = Maximum.CompareTo(toCheck);

                if( comparison > 0 ) return false;
                if( IncludeMaximumEqual && comparison == 0 ) return true;
            }

            return false;
        }

        public string GetErrorMessage( T toCheck )
        {
            if( IsValid( toCheck ) )
                return null;

            var sb = new StringBuilder();

            if (MinimumSet)
            {
                var comparison = Minimum.CompareTo(toCheck);

                if( comparison >= 0 )
                {
                    sb.Append( $"{toCheck} is " );

                    if( IncludeMinimumEqual && comparison == 0 )
                        sb.Append( ">=" );
                    else sb.Append( ">" );

                    sb.Append( $" {Minimum}" );
                }
            }

            if (MaximumSet)
            {
                var comparison = Maximum.CompareTo(toCheck);

                if( comparison <= 0 )
                {
                    sb.Append( sb.Length > 0 ? " and " : $"{toCheck} is " );

                    if( IncludeMaximumEqual && comparison == 0 )
                        sb.Append( "<=" );
                    else sb.Append( "<" );

                    sb.Append( $" {Maximum}" );
                }
            }

            return sb.Length == 0 ? null : sb.ToString();
        }
    }
}