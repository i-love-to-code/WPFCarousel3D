namespace SamNoble.Wpf.Controls.Carousel3D
{
    using SamNoble.Wpf.Controls.Carousel3D.DrWpf;
    using SamNoble.Wpf.Controls.Carousel3D.Extensions;
    using SamNoble.Wpf.Controls.Carousel3D.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Media3D;

    public class Carousel3DPanel : LogicalPanel
    {
        #region [ Constants ]

        private const double HALF_PI = Math.PI * 0.5;
        private const double TWO_PI = Math.PI * 2d;
        private const double PI_BY_180 = 180d / Math.PI;

        #endregion

        #region [ Fields ]

        private readonly Viewport3D viewport3D;

        // The geometry and material we use for our 2D elements to paint them on to 3D objects.
        private readonly MeshGeometry3D geometry;
        private readonly Material material;

        // Properties of the ellipse which only need to be calculated when the ellipse changes.
        private double ellipseCircumference;
        private double ellipseCircumferenceReciprocal;
        private Rect ellipseRect;

        private Dictionary<UIElement, Viewport2DVisual3D> elementModelMap;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Static constructor which overrides default metadata for
        /// the UIElement.ClipToBoundsProperty, setting it to true.
        /// This ensures that the content of the control is not rendered
        /// if it is outside the bounds of the control.
        /// </summary>
        static Carousel3DPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(Carousel3DPanel),
                                      new FrameworkPropertyMetadata(true,
                                            FrameworkPropertyMetadataOptions.Inherits));
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="Carousel3DPanel"/> class.
        /// </summary>
        public Carousel3DPanel()
        {
            this.elementModelMap = new Dictionary<UIElement, Viewport2DVisual3D>();

            this.geometry = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(new[] { new Point3D(-1, -1, 0), new Point3D(1, -1, 0), new Point3D(1, 1, 0), new Point3D(-1, 1, 0) }),
                TextureCoordinates = new PointCollection(new[] { new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0) }),
                TriangleIndices = new Int32Collection(new[] { 0, 1, 2, 0, 2, 3 })
            };

            this.geometry.Freeze();

            this.material = new DiffuseMaterial();
            Viewport2DVisual3D.SetIsVisualHostMaterial(this.material, true);
            this.material.Freeze();

            this.viewport3D = new Viewport3D();
            this.viewport3D.Camera = this.Camera;
            this.viewport3D.Children.Add(new ModelVisual3D() { Content = this.Light });

            //base.AddVisualChild(this.viewport3D);

            EllipseSizeChanged(this, new DependencyPropertyChangedEventArgs());
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the X coordinate of the layout ellipse's origin.
        /// This property is backed by the dependency property <see cref="EllipseCentreXProperty"/>.
        /// </summary>
        public double EllipseCentreX
        {
            get { return (double)GetValue(EllipseCentreXProperty); }
            set { SetValue(EllipseCentreXProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseCentreX"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseCentreXProperty
            = DependencyProperty.Register("EllipseCentreX", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the Y coordinate of the layout ellipse's origin.
        /// This property is backed by the dependency property <see cref="EllipseCentreYProperty"/>.
        /// </summary>
        public double EllipseCentreY
        {
            get { return (double)GetValue(EllipseCentreYProperty); }
            set { SetValue(EllipseCentreYProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseCentreY"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseCentreYProperty
            = DependencyProperty.Register("EllipseCentreY", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the Z coordinate of the layout ellipse's origin.
        /// This property is backed by the dependency property <see cref="EllipseCentreZProperty"/>.
        /// </summary>
        public double EllipseCentreZ
        {
            get { return (double)GetValue(EllipseCentreZProperty); }
            set { SetValue(EllipseCentreZProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseCentreZ"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseCentreZProperty
            = DependencyProperty.Register("EllipseCentreZ", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the height of the ellipse used to layout child content.
        /// This property is backed by the dependency property <see cref="EllipseHeightProperty"/>.
        /// </summary>
        public double EllipseHeight
        {
            get { return (double)GetValue(EllipseHeightProperty); }
            set { SetValue(EllipseHeightProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseHeight"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseHeightProperty
            = DependencyProperty.Register("EllipseHeight", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsArrange, EllipseSizeChanged));

        /// <summary>
        /// Gets or sets the width of the ellipse used to layout child content.
        /// This property is backed by the dependency property <see cref="EllipseWidthProperty"/>.
        /// </summary>
        public double EllipseWidth
        {
            get { return (double)GetValue(EllipseWidthProperty); }
            set { SetValue(EllipseWidthProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseWidth"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseWidthProperty
            = DependencyProperty.Register("EllipseWidth", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(300d, FrameworkPropertyMetadataOptions.AffectsArrange, EllipseSizeChanged));

        /// <summary>
        /// Gets or sets the angle of the layout ellipse's rotation about the x-axis.
        /// This property is backed by the dependency property <see cref="EllipseRotationXProperty"/>.
        /// </summary>
        public double EllipseRotationX
        {
            get { return (double)GetValue(EllipseRotationXProperty); }
            set { SetValue(EllipseRotationXProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseRotationX"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseRotationXProperty
            = DependencyProperty.Register("EllipseRotationX", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(70d, FrameworkPropertyMetadataOptions.AffectsArrange, null, CoerceRotationPropertyCallback));

        /// <summary>
        /// Gets or sets the angle of the layout ellipse's rotation about the y-axis.
        /// This property is backed by the dependency property <see cref="EllipseRotationYProperty"/>.
        /// </summary>
        public double EllipseRotationY
        {
            get { return (double)GetValue(EllipseRotationYProperty); }
            set { SetValue(EllipseRotationYProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseRotationY"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseRotationYProperty
            = DependencyProperty.Register("EllipseRotationY", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange, null, CoerceRotationPropertyCallback));

        /// <summary>
        /// Gets or sets the angle of the layout ellipse's rotation about the x-axis.
        /// This property is backed by the dependency property <see cref="EllipseRotationZProperty"/>.
        /// </summary>
        public double EllipseRotationZ
        {
            get { return (double)GetValue(EllipseRotationZProperty); }
            set { SetValue(EllipseRotationZProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="EllipseRotationZ"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty EllipseRotationZProperty
            = DependencyProperty.Register("EllipseRotationZ", typeof(double), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsArrange, null, CoerceRotationPropertyCallback));

        /// <summary>
        /// Gets or sets the a value indicating whether support for transparent items is required. If true,
        /// items are sorted to ensure correct rendering.
        /// This property is backed by the dependency property <see cref="TransparencySupportRequiredProperty"/>.
        /// </summary>
        public bool TransparencySupportRequired
        {
            get { return (bool)GetValue(TransparencySupportRequiredProperty); }
            set { SetValue(TransparencySupportRequiredProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="TransparencySupportRequired"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty TransparencySupportRequiredProperty
            = DependencyProperty.Register("TransparencySupportRequired", typeof(bool), typeof(Carousel3DPanel),
                                          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the a the camera for the scene.
        /// This property is backed by the dependency property <see cref="CameraProperty"/>.
        /// </summary>
        public Camera Camera
        {
            get { return (Camera)this.GetValue(CameraProperty); }
            set { this.SetValue(CameraProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="Camera"/>.
        /// Changes to this property cause the control to arrange itself, indicated with
        /// the <see cref="FrameworkPropertyMetadataOptions"/>.AffectsArrange value.
        /// </summary>
        public static readonly DependencyProperty CameraProperty
            = DependencyProperty.Register("CameraProperty", typeof(Camera), typeof(Carousel3DPanel),
            new FrameworkPropertyMetadata(new PerspectiveCamera(new Point3D(0, 0, 1500), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 45), FrameworkPropertyMetadataOptions.AffectsArrange, CameraPropertyValueChanged));

        /// <summary>
        /// Gets or sets the a the light for the scene.
        /// This property is backed by the dependency property <see cref="CameraProperty"/>.
        /// </summary>
        public Light Light
        {
            get { return (Light)this.GetValue(LightProperty); }
            set { this.SetValue(LightProperty, value); }
        }

        /// <summary>
        /// Dependency property backing store for <see cref="Light"/>.
        /// </summary>
        public static readonly DependencyProperty LightProperty
            = DependencyProperty.Register("LightProperty", typeof(Light), typeof(Carousel3DPanel),
                                        new FrameworkPropertyMetadata(new AmbientLight() { Color = Colors.White }, LightPropertyValueChanged));

        private static void CameraPropertyValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var source = sender as Carousel3DPanel;

            if (source != null && source.viewport3D != null)
            {
                source.viewport3D.Camera = e.NewValue as Camera;
            }
        }

        private static void LightPropertyValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var source = sender as Carousel3DPanel;

            if (source != null && source.viewport3D != null)
            {
                // First remove the old light
                if (e.OldValue != null && source.viewport3D.Children.Contains(e.OldValue))
                {
                    var oldModelObject = source.viewport3D
                                                    .Children.OfType<ModelVisual3D>()
                                                    .FirstOrDefault(c => c.Content == e.OldValue);

                    source.viewport3D.Children.Remove(oldModelObject);
                }

                source.viewport3D.Children.Add(new ModelVisual3D() { Content = e.NewValue as Model3D });
            }
        }

        private static void EllipseSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as Carousel3DPanel;

            if (panel == null)
            {
                return;
            }

            panel.ellipseCircumference = MathHelper.CalculateEllipseCircumferenceRamanujan2(panel.EllipseWidth, panel.EllipseHeight);
            panel.ellipseCircumferenceReciprocal = 1.0 / panel.ellipseCircumference;
        }

        private static object CoerceRotationPropertyCallback(DependencyObject sender, object baseValue)
        {
            if (baseValue == DependencyProperty.UnsetValue)
            {
                return baseValue;
            }

            var value = (double)baseValue % 360.0;

            if (value < 0)
            {
                value += 360.0;
            }

            return value;
        }

        #endregion

        #region [ Public Methods ]

        public void RotateRight(bool animated)
        {
            // If there's only 1 item here, there's nothing to rotate.
            if (base.Children.Count < 2)
            {
                return;
            }

            Viewport2DVisual3D frontMostItem = null;

            if (!this.TransparencySupportRequired)
            {
                // The children are not sorted by anything.
                frontMostItem  = this.viewport3D.Children.OfType<Viewport2DVisual3D>()
                                                         .OrderByDescending(c => c.GetTransform3D<TranslateTransform3D>().OffsetZ)
                                                         .First();
            }
            else
            {
                // We know the viewport children are sorted by depth, so the last item is 
                // the front most item.
                frontMostItem = this.viewport3D.Children.Last() as Viewport2DVisual3D;
            }

            var frontIndex = this.Children.IndexOf(frontMostItem.Visual as UIElement);
            var nextIndex = (frontIndex + 1) % this.Children.Count;
            
            this.AnimateIntoView(base.Children[nextIndex], animated);
        }

        public void RotateLeft(bool animated)
        {
            // If there's only 1 item here, there's nothing to rotate.
            if (base.Children.Count < 2)
            {
                return;
            }

            Viewport2DVisual3D frontMostItem = null;

            if (!this.TransparencySupportRequired)
            {
                // The children are not sorted by anything.
                frontMostItem = this.viewport3D.Children.OfType<Viewport2DVisual3D>()
                                                         .OrderByDescending(c => c.GetTransform3D<TranslateTransform3D>().OffsetZ)
                                                         .First();
            }
            else
            {
                // We know the viewport children are sorted by depth, so the last item is 
                // the front most item.
                frontMostItem = this.viewport3D.Children.Last() as Viewport2DVisual3D;
            }

            var frontIndex = this.Children.IndexOf(frontMostItem.Visual as UIElement);
            var nextIndex = frontIndex - 1 < 0 ? this.Children.Count - 1 : frontIndex - 1;

            this.AnimateIntoView(base.Children[nextIndex], animated);
        }

        public void AnimateIntoView(UIElement element, bool animated)
        {
            if (!this.elementModelMap.ContainsKey(element))
            {
                return;
            }

            var nextItem = this.elementModelMap[element];

            // Calculate the angle between it and the front of the ellipse (i.e. the bottom)
            var frontOfEllipse = MathHelper.GetPointOnEllipse(ellipseRect, -HALF_PI);
            var frontPosition = MathHelper.RotatePoint3D(new Point3D(frontOfEllipse.X, frontOfEllipse.Y, 0d), -EllipseRotationX, 0d, 0d);

            var itemTransform = nextItem.GetTransform3D<TranslateTransform3D>();
            var itemPosition = new Point3D(itemTransform.OffsetX, itemTransform.OffsetY, itemTransform.OffsetZ);

            var vec1 = new Vector3D(frontPosition.X, frontPosition.Y, frontPosition.Z);
            var vec2 = new Vector3D(itemPosition.X, itemPosition.Y, itemPosition.Z);

            vec1.Normalize();
            vec2.Normalize();

            var direction = Math.Sign(Vector3D.CrossProduct(vec1, vec2).Y);

            // Calculate the dot product of the two
            var angle = Vector3D.AngleBetween(vec1, vec2) * direction;

            if (animated)
            {
                var doubleAnimation = new DoubleAnimation(this.EllipseRotationZ - angle, TimeSpan.FromSeconds(1), FillBehavior.Stop);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Carousel3DPanel.EllipseRotationZProperty));

                var storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                storyboard.Completed += (o, s) => { this.EllipseRotationZ = this.EllipseRotationZ; };
                storyboard.FillBehavior = FillBehavior.Stop;

                storyboard.Begin(this);
            }
            else
            {
                this.EllipseRotationZ -= angle;
            }
        }

        #endregion

        #region [ Protected Methods ]

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = new Rect(new Point(), finalSize);

            this.viewport3D.Arrange(rect);

            if (this.Children.Count == 0)
            {
                return finalSize;
            }

            // Record the current furthest object.
            // As we process the items, record the new furthest object.
            // Did they change? If so, we need to resort.
            var furthestItem = this.viewport3D.Children.OfType<Viewport2DVisual3D>().First();
            var furthestItemTransform = furthestItem.GetTransform3D<TranslateTransform3D>();
            var furthestDepth = furthestItemTransform == null ? 0d : furthestItemTransform.OffsetZ;

            bool depthSortRequired = false;

            // Calculate the spacing between each item.
            var itemSpacing = this.ellipseCircumference / this.Children.Count;

            this.ellipseRect = new Rect(this.EllipseCentreX, this.EllipseCentreY, this.EllipseWidth, this.EllipseHeight);

            foreach (var kvp in this.elementModelMap)
            {
                var child = kvp.Value;
                var element = kvp.Key;

                // We want the item as a FrameworkElement so that we can get its height and width.
                var i = this.Children.IndexOf(element);

                element.Arrange(new Rect(new Point(0, 0), finalSize));

                // Work out how far around the ellipse we are
                double theta = TWO_PI * ((i * itemSpacing) * this.ellipseCircumferenceReciprocal);

                // Offset so the ellipse starts at the top.
                theta -= HALF_PI;

                // Now to calculate the point on the edge of the ellipse for the current rotation.
                Point p = MathHelper.GetPointOnEllipse(ellipseRect, theta);

                // We now have the x and the y values of the target point on an ellipse in 2D. 
                // We need to rotate this position by the same amount that the ellipse has been
                // rotated so that we get the correct 3D position on the ellipse
                var rotatedPoint = MathHelper.RotatePoint3D(new Point3D(p.X, p.Y, 0d), -this.EllipseRotationX, this.EllipseRotationY, this.EllipseRotationZ);

                // Create a 3D translation transformation so that the button appears in the correct place.
                var translate = new TranslateTransform3D(rotatedPoint.X, rotatedPoint.Y, rotatedPoint.Z);

                if (rotatedPoint.Z <= furthestDepth && child != furthestItem)
                {
                    depthSortRequired = true;
                }

                // Add the new translation in to the transform group.
                child.AssertTransform3D<TranslateTransform3D>(translate);
            }

            if (this.TransparencySupportRequired && depthSortRequired)
            {
                this.DepthSortChildren();
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.VisualChildrenCount == 0)
            {
                base.AddVisualChild(this.viewport3D);
            }

            this.viewport3D.Measure(availableSize);

            // Iterate all the children of the inner viewport.
            foreach (var visualChild in viewport3D.Children.OfType<Viewport2DVisual3D>())
            {
                // Get the inner UIElement
                var element = visualChild.Visual as UIElement;

                element.Measure(availableSize);

                // Create a scale transform so that the control appears the right size
                ScaleTransform3D scaleTransform
                    = new ScaleTransform3D(element.DesiredSize.Width, element.DesiredSize.Height, 0d);

                // Add the scale transform in to the Viewport2DVisual3D's transform group.
                visualChild.AssertTransform3D<ScaleTransform3D>(scaleTransform);
            }

            return this.viewport3D.DesiredSize;
        }

        protected override void OnLogicalChildrenChanged(UIElement elementAdded, UIElement elementRemoved)
        {
            // Do not create a model for the viewport.
            if (elementAdded == this.viewport3D)
            {
                return;
            }

            if (elementAdded != null && !this.elementModelMap.ContainsKey(elementAdded))
            {
                var wrappedElement = this.WrapUIElement(elementAdded);
                this.viewport3D.Children.Add(wrappedElement);
                this.elementModelMap[elementAdded] = wrappedElement;
            }

            if (elementRemoved != null && this.elementModelMap.ContainsKey(elementRemoved))
            {
                var wrapper = this.elementModelMap[elementRemoved];

                this.viewport3D.Children.Remove(wrapper);

                wrapper.Visual = null;
            }

            this.InvalidateMeasure();
            this.InvalidateArrange();
        }

        #endregion

        #region [ Private Methods ]

        /// <summary>
        /// Sorts the children of the viewport by depth from furthest to 
        /// nearest from the camera to ensure proper handling of
        /// transparent objects.
        /// </summary>
        private void DepthSortChildren()
        {
            var temp = new List<Visual3D>(this.viewport3D.Children);

            temp.Sort(new SimpleDistanceComparer());

            this.viewport3D.Children.Clear();

            foreach (var child in temp)
            {
                this.viewport3D.Children.Add(child);
            }
        }

        /// <summary>
        /// Small helper method to wrap a Visual in a Viewport2DVisual3D.
        /// </summary>
        /// <param name="wrapMe"></param>
        /// <returns></returns>
        private Viewport2DVisual3D WrapUIElement(Visual wrapMe)
        {
            Viewport2DVisual3D result = new Viewport2DVisual3D();

            result.Geometry = this.geometry;
            result.Material = this.material;
            result.Visual = wrapMe;

            return result;
        }

        #endregion
    }
}