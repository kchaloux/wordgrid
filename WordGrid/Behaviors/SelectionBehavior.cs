using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using WordGrid.Views;
using static System.Math;

namespace WordGrid.Behaviors
{
    public class SelectionBehavior : Behavior<FrameworkElement>
    {
        #region SelectedItems Property

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IList), typeof(SelectionBehavior), new PropertyMetadata(default(IList)));

        public IList Items
        {
            get => (IList)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        #endregion

        #region IsDragging Attached Property

        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached(
            "IsDragging", typeof(bool), typeof(SelectionBehavior), new PropertyMetadata(default(bool)));

        public static void SetIsDragging(DependencyObject element, bool value)
        {
            element.SetValue(IsDraggingProperty, value);
        }

        public static bool GetIsDragging(DependencyObject element)
        {
            return (bool)element.GetValue(IsDraggingProperty);
        }

        #endregion

        #region X Attached Property

        public static readonly DependencyProperty XProperty = DependencyProperty.RegisterAttached(
            "X", typeof(double), typeof(SelectionBehavior), new PropertyMetadata(default(double)));

        public static void SetX(DependencyObject element, double value)
        {
            element.SetValue(XProperty, value);
        }

        public static double GetX(DependencyObject element)
        {
            return (double)element.GetValue(XProperty);
        }

        #endregion

        #region Y Attached Property

        public static readonly DependencyProperty YProperty = DependencyProperty.RegisterAttached(
            "Y", typeof(double), typeof(SelectionBehavior), new PropertyMetadata(default(double)));

        public static void SetY(DependencyObject element, double value)
        {
            element.SetValue(YProperty, value);
        }

        public static double GetY(DependencyObject element)
        {
            return (double)element.GetValue(YProperty);
        }

        #endregion

        #region Width Attached Property

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width", typeof(double), typeof(SelectionBehavior), new PropertyMetadata(default(double)));

        public static void SetWidth(DependencyObject element, double value)
        {
            element.SetValue(WidthProperty, value);
        }

        public static double GetWidth(DependencyObject element)
        {
            return (double)element.GetValue(WidthProperty);
        }

        #endregion

        #region Height Attached Property

        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached(
            "Height", typeof(double), typeof(SelectionBehavior), new PropertyMetadata(default(double)));

        public static void SetHeight(DependencyObject element, double value)
        {
            element.SetValue(HeightProperty, value);
        }

        public static double GetHeight(DependencyObject element)
        {
            return (double)element.GetValue(HeightProperty);
        }

        #endregion

        private Point _origin;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += OnPreviewMouseDown;
            AssociatedObject.MouseMove += OnPreviewMouseMove;
            AssociatedObject.MouseUp += OnPreviewMouseUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnPreviewMouseDown;
            AssociatedObject.MouseMove -= OnPreviewMouseMove;
            AssociatedObject.MouseUp -= OnPreviewMouseUp;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GetIsDragging(AssociatedObject))
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _origin = Mouse.GetPosition((IInputElement)e.Source);
                SetX(AssociatedObject, _origin.X);
                SetY(AssociatedObject, _origin.Y);
                SetIsDragging(AssociatedObject, true);
            }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!GetIsDragging(AssociatedObject))
            {
                return;
            }
            var pos = Mouse.GetPosition((IInputElement)e.Source);
            SetX(AssociatedObject, Min(pos.X, _origin.X));
            SetY(AssociatedObject, Min(pos.Y, _origin.Y));
            SetWidth(AssociatedObject, Abs(pos.X - _origin.X));
            SetHeight(AssociatedObject, Abs(pos.Y - _origin.Y));
            UpdateSelectedItems();
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!GetIsDragging(AssociatedObject))
            {
                return;
            }
            UpdateSelectedItems();
            SetIsDragging(AssociatedObject, false);
            SetX(AssociatedObject, -1);
            SetY(AssociatedObject, -1);
            SetWidth(AssociatedObject, 0);
            SetHeight(AssociatedObject, 0);
            _origin = new Point();
        }

        private void UpdateSelectedItems()
        {
            if (Items == null)
            {
                return;
            }
            
            var selectedElements = new HashSet<object>();
            HitTestResultBehavior ResultCallback(HitTestResult result)
            {
                var element = (result.VisualHit as FrameworkElement);
                if (element?.DataContext != null)
                {
                    selectedElements.Add(element);
                }
                return HitTestResultBehavior.Continue;
            }
            HitTestFilterBehavior FilterCallback(DependencyObject target) => HitTestFilterBehavior.Continue;

            var area = new Rect(
                new Point(GetX(AssociatedObject), GetY(AssociatedObject)),
                new Size(GetWidth(AssociatedObject), GetHeight(AssociatedObject)));

            var hitTestParameters = Abs(area.Width) < 1E-12 || Abs(area.Height) < 1E-12
                ? (HitTestParameters)new PointHitTestParameters(Mouse.GetPosition(AssociatedObject))
                : new GeometryHitTestParameters(new RectangleGeometry(area));

            VisualTreeHelper.HitTest(AssociatedObject, FilterCallback, ResultCallback, hitTestParameters);

            var currentItems = Items.OfType<object>().ToList();
            var selectedItems = selectedElements.Select(x => ((FrameworkElement)x).DataContext).ToList();

            var toRemove = currentItems.Except(selectedItems).ToList();
            foreach (var item in toRemove)
            {
                Items.Remove(item);
            }
            
            var toAdd = selectedItems.Except(currentItems).ToList();
            foreach (var item in toAdd)
            {
                Items.Add(item);
            }
        }
    }
}
