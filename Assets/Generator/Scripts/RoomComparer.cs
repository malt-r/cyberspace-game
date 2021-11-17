using System.Collections.Generic;

public partial class DungeonGenerator
{
    private class RoomComparer : IComparer<Room>
    {
        public int Compare(Room x, Room y)
        {
            // x less than y -> neg val
            // eq -> 0
            // x greater than y -> pos val

            if (x == y)
            {
                return 0;
            }

            var xMarker = x.GetFirstStoryMarker();
            var yMarker = y.GetFirstStoryMarker();

            // null checks
            if (null == xMarker && null != yMarker)
            {
                return -1;
            }

            if (null != xMarker && null == yMarker)
            {
                return 1;
            }

            if (null == xMarker && null == yMarker)
            {
                return 0;
            }

            // x is not relevant for story, but y is
            if (!xMarker.RelevantForStory && yMarker.RelevantForStory)
            {
                return -1;
            }

            // x is relevant for story, but y is not
            if (xMarker.RelevantForStory && !yMarker.RelevantForStory)
            {
                return 1;
            }

            // both are not relevant to story
            if (!xMarker.RelevantForStory && !yMarker.RelevantForStory)
            {
                return 0;
            }

            return xMarker.IndexInStory.CompareTo(yMarker.IndexInStory);
        }
    }
}
