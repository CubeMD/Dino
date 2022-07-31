using UnityEngine;

namespace MLDebugTool.Scripts.Utilities
{
    public static class VectorExtensions
    {
        /// <summary>
        /// A useful Epsilon
        /// </summary>
        public const float EPSILON = 0.0001f;
        
        /// <summary>
        /// Returns a non-normalized projection of the supplied vector onto a plane
        /// as described by its normal
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="planeNormal">The normal that defines the plane.  Must have a length of 1.</param>
        /// <returns>The component of the vector that lies in the plane</returns>
        public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
        {
            return (vector - Vector3.Dot(vector, planeNormal) * planeNormal);
        }
        
        /// <summary>
        /// Is the vector within Epsilon of zero length?
        /// </summary>
        /// <param name="v"></param>
        /// <returns>True if the square magnitude of the vector is within Epsilon of zero</returns>
        public static bool AlmostZero(this Vector3 v)
        {
            return v.sqrMagnitude < (EPSILON * EPSILON);
        }
    }
}