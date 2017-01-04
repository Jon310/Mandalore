using System.Numerics;
using JetBrains.Annotations;
using Mandalore.Helpers;
using Styx;
using Styx.Helpers;
using Styx.WoWInternals.WoWObjects;
using M = Mandalore.Movement.Movement;

namespace Mandalore.Math
{
    [UsedImplicitly]
    internal class Calculators
    {
        private static readonly LocalPlayer Me = StyxWoW.Me;

        #region Rotation

        // Rotation
        public static float _rotation(WoWObject obj)
        {
            return obj.RotationDegrees;
        }

        public static float _rot(WoWObject obj)
        {
            return Me.Rotation;
        }

        public static double _radians(WoWObject obj)
        {
            return _rotation(obj)*(System.Math.PI/180);
        }

        public static double _theta(WoWObject obj)
        {
            return System.Math.Atan(_Y(obj)/_X(obj)); // Radians
        }

        public static double _thetadeg(WoWObject obj)
        {
            return RadtoDeg(_theta(obj));
        }

        public static double GetDegree
        {
            get
            {
                var d = System.Math.Atan2((Me.CurrentTarget.Y - Me.Y), (Me.CurrentTarget.X - Me.X));

                var r = d - Me.CurrentTarget.Rotation; 	  // subtracting object rotation from absolute rotation
                if (r < 0)
                    r += (System.Math.PI * 2);

                return WoWMathHelper.RadiansToDegrees((float)r);
            }
        }

        #endregion

        #region Position

        //////////////////
        //// Position ////
        //////////////////

        // Cartesian
        public static double _X(WoWObject obj) 
        {
            return obj.X;
        }

        public static double _Y(WoWObject obj)
        {
            return obj.Y;
        }

        public static float _Z(WoWObject obj)
        {
            return obj.Z;
        }

        /// <summary>
        /// Relative X. (coordinate system centered on obj1 at (0,0,0)
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>obj2 - obj1</returns>
        public static float _relX(WoWObject obj1, WoWObject obj2)
        {
            return obj2.X - obj1.X;
        }
        
        /// <summary>
        /// Relative Y. (coordinate system centered on obj1 at (0,0,0)
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>obj2 - obj1</returns>
        public static float _relY(WoWObject obj1, WoWObject obj2)
        {
            return obj2.Y - obj1.Y;
        }

        /// <summary>
        /// Relative Z. (coordinate system centered on obj1 at (0,0,0)
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>obj2 - obj1</returns>
        public static float _relZ(WoWObject obj1, WoWObject obj2)
        {
            return obj2.Z - obj1.Z;
        }

        // Polar (R, theta, Z)
        public static double _R(WoWObject obj1, WoWObject obj2)
        {
            return System.Math.Sqrt(System.Math.Pow(_relX(obj1, obj2), 2) + System.Math.Pow(_relY(obj1, obj2), 2)); // (x^2 + Y^2)^(1/2)
        }
        // theta and Z are the same as Rotation and Cartesian

        #endregion

        #region Vectors        

        /// <summary>
        /// The Vector from obj1 to obj2
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>A Vector <x, y, z></returns>
        public static Vector3 _obj1ToObj2(WoWObject obj1, WoWObject obj2)
        {
            return new Vector3(_relX(obj1, obj2), _relY(obj1, obj2), _relZ(obj1, obj2));
        }


        //public static Vector3 _unitVector(WoWObject obj1, WoWObject obj2)
        //{
        //    return _obj1ToObj2(obj1, obj2) / _obj1ToObj2(obj1, obj2).Magnitude; // .Magnitude not available.
        //}

        public static Vector3 RotationMatrix(Vector3 targetVector3, double theta)
        {
            
            var xcoord = (System.Math.Cos(theta) * targetVector3.X) - (System.Math.Sin(theta) * targetVector3.X);
            var ycoord = (System.Math.Sin(theta) * targetVector3.Y) + (System.Math.Cos(theta) * targetVector3.Y);


            return new Vector3((float) xcoord, (float) ycoord, Me.Z);
        }


        #endregion

        #region Conversions

        static double RadtoDeg(double rads)
        {
            return (rads*(180/System.Math.PI));
        }

        static double DegtoRad(float deg)
        {
            return (deg*(System.Math.PI/180));
        }

        #endregion
    }
}
