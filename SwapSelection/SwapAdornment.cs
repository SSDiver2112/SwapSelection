using System;
//using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Shapes;

namespace SwapSelection
{
    /// <summary>
    /// SwapAdornment places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class SwapAdornment
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView view;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapAdornment"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public SwapAdornment(IWpfTextView view)
        {
            if (view == null)
            {
               throw new ArgumentNullException("view");
            }

            this.layer = view.GetAdornmentLayer("SwapAdornment");

            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;
            var msel = this.view.GetMultiSelectionBroker();
            msel.MultiSelectionSessionChanged += this.OnMultiSelectionSessionChanged;
        }

        void OnMultiSelectionSessionChanged(object sender, EventArgs e)
        {
            CheckMultiSelect();
        }

        private void CheckMultiSelect()
        {
            var mItems = view.Selection.SelectedSpans;
            if (mItems.Count == 2)
            {
                var selected1 = mItems[0].GetText();
                var selected2 = mItems[1].GetText();
                if ((selected1.Length > 0 && selected2.Length > 0) && selected1 != selected2)
                {
                    this.CreateVisuals();
                }
                else
                {
                    this.layer.RemoveAllAdornments();
                }
            }
            else
            {
                this.layer.RemoveAllAdornments();
            }
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            CheckMultiSelect();
        }

        /// <summary>
        /// Adds the scarlet box behind the 'a' characters within the given line
        /// </summary>
        /// <param name="line">Line to add the adornments</param>
        private void CreateVisuals()
        {
            if (VSPackage.Options != null)
            { 
            this.layer.RemoveAllAdornments();
             var swapColor = (Color)ColorConverter.ConvertFromString(VSPackage.Options.ArrowColor);

            var swapPenBrush = new SolidColorBrush(swapColor);
            swapPenBrush.Freeze();
            var swapPen = new Pen(swapPenBrush, 2);
            swapPen.Freeze();

            IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;
            int whichSwap = 0;
            foreach (SnapshotSpan snapshotSpan in view.Selection.SelectedSpans)
            {

                Geometry geometry = textViewLines.GetMarkerGeometry(snapshotSpan);
                    if (geometry != null)
                    {
                        Rect swapRect = new Rect(geometry.Bounds.Location, geometry.Bounds.Size);
                        swapRect.Inflate(4, 4);
                        GeometryGroup swapLine = new GeometryGroup();

                        switch (VSPackage.Options.AdornmentType)
                        {
                            case 1:
                                swapLine = MakeSwapArrow(swapRect);
                                break;
                            case 2:
                                swapLine = MakeSwapBracket(swapRect);
                                break;
                            case 3:
                                swapLine = MakeSwapBadge(swapRect);
                                break;
                        }

                        var drawing = new GeometryDrawing(new SolidColorBrush(swapColor), swapPen, swapLine);
                        drawing.Freeze();

                        var drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        var swapImage = new Image
                        {
                            Source = drawingImage,
                        };

                        if (whichSwap == 1)
                        {
                            Canvas.SetLeft(swapImage, swapRect.Right - drawing.Bounds.Width);
                            Canvas.SetTop(swapImage, swapRect.Top);

                            swapImage.RenderTransformOrigin = new Point(0.5, 0.5);
                            var flipTrans = new ScaleTransform(-1, 1);

                            swapImage.RenderTransform = flipTrans;
                        }
                        else
                        {
                            Canvas.SetLeft(swapImage, swapRect.Left);
                            Canvas.SetTop(swapImage, swapRect.Top);
                        }

                        this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, snapshotSpan, null, swapImage, null);

                        whichSwap = 1;
                    }
                }
            }
        }

        GeometryGroup MakeSwapBadge(Rect swapRect)
        {
            GeometryGroup gmgp = new GeometryGroup();
            PathGeometry ArrowBadgeGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            PathFigure pathFigureArrowBadge = new PathFigure();

            LineSegment ArrowBadgeTop = new LineSegment();
            LineSegment ArrowBadgeBottom = new LineSegment();
            pathFigureArrowBadge.IsFilled = true;
            pathFigureArrowBadge.IsClosed = true;
            pathFigureArrowBadge.StartPoint = new Point(swapRect.X + 10, swapRect.Top - 2);
            ArrowBadgeTop.Point = new Point(swapRect.X + 15, swapRect.Top);
            ArrowBadgeBottom.Point = new Point(swapRect.X + 10, swapRect.Top + 2);

            pathFigureArrowBadge.Segments.Add(ArrowBadgeTop);
            pathFigureArrowBadge.Segments.Add(ArrowBadgeBottom);
            ArrowBadgeGeometry.Figures.Add(pathFigureArrowBadge);

            gmgp.Children.Add(ArrowBadgeGeometry);

            return gmgp;
        }

        GeometryGroup MakeSwapBracket(Rect swapRect)
        {
            GeometryGroup gmgp = new GeometryGroup();
            PathGeometry SquareBracketGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            PathGeometry ArrowBracketGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            PathFigure pathFigureSquareBracket = new PathFigure();
            PathFigure pathFigureArrowBracket = new PathFigure();

            LineSegment SquareBracketTop = new LineSegment();
            LineSegment SquareBracketSide = new LineSegment();
            LineSegment SquareBracketBottom = new LineSegment();


            pathFigureSquareBracket.IsFilled = false;
            pathFigureSquareBracket.StartPoint = new Point(swapRect.X + 10, swapRect.Top);
            SquareBracketTop.Point = new Point(swapRect.X, swapRect.Top);
            SquareBracketSide.Point = new Point(swapRect.X, swapRect.Bottom - 6);
            SquareBracketBottom.Point = new Point(swapRect.X + 10, swapRect.Bottom - 6);

            pathFigureSquareBracket.Segments.Add(SquareBracketTop);
            pathFigureSquareBracket.Segments.Add(SquareBracketSide);
            pathFigureSquareBracket.Segments.Add(SquareBracketBottom);
            SquareBracketGeometry.Figures.Add(pathFigureSquareBracket);


            LineSegment ArrowBracketTop = new LineSegment();
            LineSegment ArrowBracketBottom = new LineSegment();
            pathFigureArrowBracket.IsFilled = true;
            pathFigureArrowBracket.IsClosed = true;
            pathFigureArrowBracket.StartPoint = new Point(swapRect.X + 10, swapRect.Top - 2);
            ArrowBracketTop.Point = new Point(swapRect.X + 15, swapRect.Top);
            ArrowBracketBottom.Point = new Point(swapRect.X + 10, swapRect.Top + 2);

            pathFigureArrowBracket.Segments.Add(ArrowBracketTop);
            pathFigureArrowBracket.Segments.Add(ArrowBracketBottom);
            ArrowBracketGeometry.Figures.Add(pathFigureArrowBracket);

            gmgp.Children.Add(SquareBracketGeometry);
            gmgp.Children.Add(ArrowBracketGeometry);

            return gmgp;
        }

        GeometryGroup MakeSwapArrow(Rect rect)
        {
            GeometryGroup gmgp = new GeometryGroup();
            PathGeometry linePathGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            PathGeometry ArrowHeadPathGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            PathFigure pathFigureCurve = new PathFigure();
            PathFigure pathFigureArrowHead = new PathFigure();
            BezierSegment curve = new BezierSegment();

            pathFigureCurve.IsFilled = false;
            pathFigureCurve.StartPoint = new Point(rect.X + 5, rect.Bottom - 1);
            curve.Point1 = new Point(rect.X - 6, rect.Y + 4);
            curve.Point2 = new Point(rect.X + 4, rect.Y - 2.5);
            curve.Point3 = new Point(rect.Right - (rect.Width / 4), rect.Y + 5);
            pathFigureCurve.Segments.Add(curve);

            linePathGeometry.Figures.Add(pathFigureCurve);

            double xDiff = curve.Point2.X - curve.Point3.X;
            double yDiff = curve.Point2.Y - curve.Point3.Y;
            double angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
            angle += 180;

            pathFigureArrowHead.StartPoint = new Point(curve.Point3.X + 4, curve.Point3.Y);
            LineSegment lineSegmentA = new LineSegment();
            lineSegmentA.Point = new Point(curve.Point3.X, curve.Point3.Y + 2);
            pathFigureArrowHead.Segments.Add(lineSegmentA);
            LineSegment lineSegmentB = new LineSegment();
            lineSegmentB.Point = new Point(curve.Point3.X, curve.Point3.Y - 2);
            pathFigureArrowHead.Segments.Add(lineSegmentB);
            LineSegment lineSegmentC = new LineSegment();
            lineSegmentC.Point = new Point(curve.Point3.X + 4, curve.Point3.Y);
            pathFigureArrowHead.Segments.Add(lineSegmentC);

            pathFigureArrowHead.IsClosed = true;

            ArrowHeadPathGeometry.Figures.Add(pathFigureArrowHead);
            RotateTransform rt2 = new RotateTransform(angle, curve.Point3.X, curve.Point3.Y);
            ArrowHeadPathGeometry.Transform = rt2;

            gmgp.Children.Add(linePathGeometry);
            gmgp.Children.Add(ArrowHeadPathGeometry);

            return gmgp;
        }

    }
}
