using FluentAssertions;
using System.Linq;
using FluentAssertions.Collections;
using FluentAssertions.Execution;

namespace System.CommandLine.Tests
{
    public static class AssertionExtensions
    {
        public static AndConstraint<GenericCollectionAssertions<T>> BeEquivalentSequenceTo<T>(
            this GenericCollectionAssertions<T> assertions,
            params object[] expectedValues)
        {
            var actualValues = assertions.Subject.ToArray();

            actualValues
                .Select(a => a.GetType())
                .Should()
                .BeEquivalentTo(expectedValues.Select(e => e.GetType()));

            using (new AssertionScope())
            {
                foreach (var tuple in actualValues
                                      .Zip(expectedValues, (actual, expected) => (actual, expected))
                                      .Where(t => t.expected.GetType().GetProperties().Any()))

                {
                    tuple.actual
                         .Should()
                         .BeEquivalentTo(tuple.expected);
                }
            }

            return new AndConstraint<GenericCollectionAssertions<T>>(assertions);
        }
    }
}
