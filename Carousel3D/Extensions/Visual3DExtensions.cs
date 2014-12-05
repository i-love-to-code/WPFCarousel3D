namespace SamNoble.Wpf.Controls.Carousel3D.Extensions
{
    using System;
    using System.Linq;
    using System.Windows.Media.Media3D;

    public static class Visual3DExtensions
    {
        /// <summary>
        ///   <para>
        ///     This method is rather cool in that it allows you to set / update a transform on a Visual3D
        ///     without having to worry about affecting existing transformations if a
        ///     Transform3DGroup is in use. 
        ///   </para>
        ///   <para>
        ///     If the <paramref name="visual3D"/>.Transform property is null,
        ///     a new <see cref="Transform3DGroup"/> will be created and <paramref name="transform"/> will
        ///     be added in to it.
        ///   </para>
        ///   <para>
        ///     If the <paramref name="visual3D"/>.Transform property is not null, and contains a transform
        ///     of type <typeparamref name="T"/>, the transform will be replaced, otherwise it will be added.
        ///   </para>
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the 3D transform class we are asserting.</typeparam>
        /// <param name="transform">The instance of the transform we are asserting.</param>
        /// <param name="visual3D">The <see cref="Visual3D"/> we want the transform to be applied to.</param>
        public static void AssertTransform3D<T>(this Visual3D visual3D, T transform) where T : Transform3D
        {
            var transform3DGroup = visual3D.Transform as Transform3DGroup;

            if (transform3DGroup == null)
            {
                transform3DGroup = new Transform3DGroup();

                if (visual3D.Transform != null && !(visual3D.Transform is T))
                {
                    transform3DGroup.Children.Add(visual3D.Transform);
                }

                visual3D.Transform = transform3DGroup;
            }

            // Given the way we are handling transforms on objects, there will only
            // be either a scale transform and or a translate transform, or neither.
            var oldTransform = transform3DGroup.Children.FirstOrDefault(c => c is T);

            // If we didn't find the target transform, add it in.
            if (oldTransform == null)
            {
                transform3DGroup.Children.Add(transform);
            }
            else
            {
                //Out with old and in with the new...
                transform3DGroup.Children.Remove(oldTransform);
                transform3DGroup.Children.Add(transform);
            }
        }

        public static T GetTransform3D<T>(this Visual3D visual) where T : Transform3D
        {
            var transformGroup = visual.Transform as Transform3DGroup;

            if (transformGroup != null)
            {
                return transformGroup.Children.FirstOrDefault(c => c is T) as T;
            }

            return visual.Transform as T;
        }
    }
}
