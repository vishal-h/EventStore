﻿namespace NEventStore.Client
{
    using NEventStore.Persistence;

    public abstract class ClientBase
    {
        private readonly IPersistStreams _persistStreams;

        protected ClientBase(IPersistStreams persistStreams)
        {
            _persistStreams = persistStreams;
        }

        protected IPersistStreams PersistStreams
        {
            get { return _persistStreams; }
        }


        /// <summary>
        /// Observe commits from the sepecified checkpoint token. If the token is null,
        ///  all commits from the beginning will be observed.
        /// </summary>
        /// <param name="checkpointToken">The checkpoint token.</param>
        /// <returns>An <see cref="IObserveCommits"/> instance.</returns>
        public abstract IObserveCommits ObserveFrom(string checkpointToken = null);
    }
}