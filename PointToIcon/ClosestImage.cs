using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointToIcon
{
    public class ClosestImage
    {

        private static List<int> lastPicked = new List<int>();

        // This finds the closest position to where the desktop icon is and returns the index of that position
        private static int BruteClosest(Position iconPosition, List<Position> positions)
        {
            int index = 0;
            float closest = float.MaxValue;

            for(int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                float distance = Position.DistancePositions(pos, iconPosition);
                if (distance < closest && !lastPicked.Contains(i))
                {
                    closest = distance;
                    index = i;
                }
            }
            lastPicked.Clear();

            return index;
        }

        public static int UseClosest(Position screenSize, Position iconPosition, List<Position> positions)
        {
            Position position = Position.DividePositions(iconPosition, screenSize);
            return BruteClosest(position, positions);
        }

    }
}
