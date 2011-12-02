using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace Microsoft.UI.Xaml.Controls
{
    // Rich text wrapping support
    [Windows.UI.Xaml.Markup.ContentProperty(Name = "RichTextContent")]
    public sealed class RichTextColumns : Panel
    {
        public static readonly DependencyProperty RichTextContentProperty =
            DependencyProperty.Register("RichTextContent", typeof(RichTextBlock),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));
        public static readonly DependencyProperty ColumnTemplateProperty =
            DependencyProperty.Register("ColumnTemplate", typeof(DataTemplate),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        public RichTextBlock RichTextContent
        {
            get { return (RichTextBlock)GetValue(RichTextContentProperty); }
            set { SetValue(RichTextContentProperty, value); }
        }

        public DataTemplate ColumnTemplate
        {
            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
            set { SetValue(ColumnTemplateProperty, value); }
        }

        private static void ResetOverflowLayout(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as RichTextColumns;
            if (target != null)
            {
                target.InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Children.Clear();
            if (RichTextContent == null) return new Size(0, 0);

            Children.Add(RichTextContent);
            RichTextContent.OverflowContentTarget = null;
            RichTextContent.Measure(availableSize);
            var maxWidth = RichTextContent.DesiredSize.Width;
            var maxHeight = RichTextContent.DesiredSize.Height;
            var hasOverflow = RichTextContent.HasOverflowContent;

            if (hasOverflow && ColumnTemplate != null)
            {
                // Create necessary overflow blocks
                RichTextBlockOverflow previousOverflow = null;
                while (hasOverflow && maxWidth < availableSize.Width)
                {
                    RichTextBlockOverflow overflow = (RichTextBlockOverflow)ColumnTemplate.LoadContent();
                    Children.Add(overflow);

                    // Establish overflow from the previous block and disable it beyond
                    if (previousOverflow == null)
                    {
                        RichTextContent.OverflowContentTarget = overflow;
                    }
                    else
                    {
                        previousOverflow.OverflowContentTarget = overflow;
                    }
                    overflow.Measure(new Size(availableSize.Width - maxWidth, availableSize.Height));
                    maxWidth += overflow.DesiredSize.Width;
                    maxHeight = Math.Max(maxHeight, overflow.DesiredSize.Height);
                    hasOverflow = overflow.HasOverflowContent;
                    previousOverflow = overflow;
                }
            }
            return new Size(maxWidth, maxHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(maxWidth, 0, child.DesiredSize.Width, finalSize.Height));
                maxWidth += child.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }
            return new Size(maxWidth, maxHeight);
        }
    }
}
