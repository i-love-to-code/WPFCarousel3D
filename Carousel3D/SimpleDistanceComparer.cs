namespace SamNoble.Wpf.Controls.Carousel3D
{
    using SamNoble.Wpf.Controls.Carousel3D.Extensions;
    using System.Collections.Generic;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Simple <see cref="IComparer"/> implementation used to determine the closest of two
    /// object in relation to the camera. Assumes that depth is related to the Z-axis only.
    /// For more advanced scenarios, the comparaer could take in to account the camera's location.
    /// </summary>
    public sealed class SimpleDistanceComparer : IComparer<Visual3D>
    {
        public int Compare(Visual3D x, Visual3D y)
        {
            var t1 = x.GetTransform3D<TranslateTransform3D>();
            var t2 = y.GetTransform3D<TranslateTransform3D>();

            if (t1 == null || t2 == null)
            {
                return 0;
            }

            return t1.OffsetZ.CompareTo(t2.OffsetZ);
        }
    }
}
