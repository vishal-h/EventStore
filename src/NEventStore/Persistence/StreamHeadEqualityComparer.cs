namespace NEventStore.Persistence
{
    using System.Collections.Generic;

    public sealed class StreamHeadEqualityComparer : IEqualityComparer<IStreamHead>
    {
        public bool Equals(IStreamHead x, IStreamHead y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null))
            {
                return false;
            }
            if (ReferenceEquals(y, null))
            {
                return false;
            }
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return string.Equals(x.StreamId, y.StreamId) && string.Equals(x.BucketId, y.BucketId);
        }

        public int GetHashCode(IStreamHead obj)
        {
            unchecked
            {
                return ((obj.StreamId != null ? obj.StreamId.GetHashCode() : 0) * 397) ^ (obj.BucketId != null ? obj.BucketId.GetHashCode() : 0);
            }
        }
    }
}