using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointToIcon
{
    // Class that just stores an X and Y position.
    public class Position
    {

        public float X;
        public float Y;

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Some maths functions to get distance between 2 positions etc.

        public static float DistancePositions(Position pos1, Position pos2)
        {
            return (float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2);
        }

        public static Position DividePositions(Position pos1, Position pos2)
        {
            return new Position(pos1.X / pos2.X, pos1.Y / pos2.Y);
        }

        public static Position MultiplyPositions(Position pos1, Position pos2)
        {
            return new Position(pos1.X * pos2.X, pos1.Y * pos2.Y);
        }

        public static Position DividePositionsByNumber(Position pos, float num)
        {
            return new Position(pos.X / num, pos.Y / num);
        }

        public static Position MultiplyPositionsByNumber(Position pos, float num)
        {
            return new Position(pos.X * num, pos.Y * num);
        }

        public static Position AddPositions(Position pos1, Position pos2)
        {
            return new Position(pos1.X + pos2.X, pos1.Y + pos2.Y);
        }

        public static Position SubtractPositions(Position pos1, Position pos2)
        {
            return new Position(pos1.X - pos2.X, pos1.Y - pos2.Y);
        }
    }
}
