namespace NEventStore.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Indicates the ability to adapt the underlying persistence infrastructure to behave like a stream of events.
    /// </summary>
    /// <remarks>
    ///     Instances of this class must be designed to be multi-thread safe such that they can be shared between threads.
    /// </remarks>
    public interface IPersistStreams : IDisposable, ICommitEvents, IAccessSnapshots
    {
        /// <summary>
        ///     Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        ///     Initializes and prepares the storage for use, if not already performed.
        /// </summary>
        /// <exception cref="StorageException" />
        /// <exception cref="StorageUnavailableException" />
        void Initialize();

        /// <summary>
        ///     Gets all commits on or after from the specified starting time.
        /// </summary>
        /// <param name="bucketId">The value which uniquely identifies bucket the stream belongs to.</param>
        /// <param name="start">The point in time at which to start.</param>
        /// <returns>All commits that have occurred on or after the specified starting time.</returns>
        /// <exception cref="StorageException" />
        /// <exception cref="StorageUnavailableException" />
        IEnumerable<ICommit> GetFrom(string bucketId, DateTime start);

        /// <summary>
        ///     Gets all commits after from the specified checkpoint. Use null to get from the beginning.
        /// </summary>
        /// <param name="checkpointToken">The checkpoint token.</param>
        /// <returns>An enumerable of Commits.</returns>
        IEnumerable<ICommit> GetFrom(string checkpointToken = null);

        /// <summary>
        /// Gets a checkpoint object that is comparable with other checkpoints from this storage engine.
        /// </summary>
        /// <param name="checkpointToken">The checkpoint token</param>
        /// <returns>A <see cref="ICheckpoint"/> instance.</returns>
        ICheckpoint GetCheckpoint(string checkpointToken = null);

        /// <summary>
        ///     Gets all commits on or after from the specified starting time and before the specified end time.
        /// </summary>
        /// <param name="bucketId">The value which uniquely identifies bucket the stream belongs to.</param>
        /// <param name="start">The point in time at which to start.</param>
        /// <param name="end">The point in time at which to end.</param>
        /// <returns>All commits that have occurred on or after the specified starting time and before the end time.</returns>
        /// <exception cref="StorageException" />
        /// <exception cref="StorageUnavailableException" />
        IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end);

        /// <summary>
        ///     Completely DESTROYS the contents of ANY and ALL streams that have been successfully persisted.  Use with caution.
        /// </summary>
        void Purge();

        /// <summary>
        ///     Completely DESTROYS the contents of ANY and ALL streams that have been successfully persisted
        ///     in the specified bucket.  Use with caution.
        /// </summary>
        void Purge(string bucketId);

        /// <summary>
        ///     Completely DESTROYS the contents and schema (if applicable) containting ANY and ALL streams that have been
        ///     successfully persisted
        ///     in the specified bucket.  Use with caution.
        /// </summary>
        void Drop();

        /// <summary>
        /// Deletes a stream.
        /// </summary>
        /// <param name="bucketId">The bucket Id from which the stream is to be deleted.</param>
        /// <param name="streamId">The stream Id of the stream that is to be deleted.</param>
        void DeleteStream(string bucketId, string streamId);
    }
}