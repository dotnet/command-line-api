// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering.Views;
using System.CommandLine.Tests.Utility;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Rendering.Tests.Views
{
    public class LayoutViewTests
    {
        [Fact]
        public void Add_child_appends_view_to_children()
        {
            var layout = new TestLayout();
            var child1 = new TestView();
            var child2 = new TestView();

            layout.Add(child1);
            layout.Add(child2);

            layout.Children.Should().BeEquivalentSequenceTo(child1, child2);
        }

        [Fact]
        public void Child_added_to_layout_registers_for_updated()
        {
            var layout = new TestLayout();
            var view = new TestView();

            layout.Add(view);

            view.RaiseUpdated();

            layout.OnChildUpdatedInvocationCount.Should().Be(1);
        }

        [Fact]
        public void Adding_null_child_throws_exception()
        {
            var layout = new TestLayout();

            Action addNullChild = () => layout.Add(null);

            addNullChild.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Removing_child_from_layout_unregisters_for_updated()
        {
            var layout = new TestLayout();
            var view = new TestView();

            layout.Add(view);
            layout.Remove(view);
            view.RaiseUpdated();

            layout.OnChildUpdatedInvocationCount.Should().Be(0);
            layout.Children.Should().BeEmpty();
        }

        [Fact]
        public void Clearing_children_removes_all_child_views()
        {
            var layout = new TestLayout();
            var view1 = new TestView();
            var view2 = new TestView();

            layout.Add(view1);
            layout.Remove(view2);

            layout.Clear();
            
            layout.Children.Should().BeEmpty();
        }

        [Fact]
        public void Layout_view_adds_children_with_collection_initializer()
        {
            var layout = new TestLayout
            {
                new TestView(),
                new TestView()
            };

            layout.Children.Count.Should().Be(2);
        }

        private class TestView : View
        {
            public override void Render(ConsoleRenderer renderer, Region region) => throw new NotImplementedException();

            public override Size Measure(ConsoleRenderer renderer, Size maxSize) => throw new NotImplementedException();

            public void RaiseUpdated() => OnUpdated();
        }

        private class TestLayout : LayoutView<View>
        {
            public override Size Measure(ConsoleRenderer renderer, Size maxSize)
            {
                throw new NotImplementedException();
            }

            public override void Render(ConsoleRenderer renderer, Region region)
            {
                throw new NotImplementedException();
            }

            public int OnChildUpdatedInvocationCount { get; set; }
            protected override void OnChildUpdated(object sender, EventArgs e)
            {
                OnChildUpdatedInvocationCount++;
                base.OnChildUpdated(sender, e);
            }
        }
    }
}
