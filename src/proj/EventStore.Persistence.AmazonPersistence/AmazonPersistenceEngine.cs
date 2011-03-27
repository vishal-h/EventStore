namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	public class AmazonPersistenceEngine : IPersistStreams
	{
		private readonly SimpleDbClient database;
		private readonly SimpleStorageClient storage;
		private int initialized;

		public AmazonPersistenceEngine(SimpleDbClient database, SimpleStorageClient storage)
		{
			this.database = database;
			this.storage = storage;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			this.storage.Dispose();
		}

		public virtual void Initialize()
		{
			if (Interlocked.Increment(ref this.initialized) > 1)
				return;

			this.database.Initialize();
			this.storage.Initialize();
		}

		public virtual IEnumerable<Commit> GetFrom(Guid streamId, int minRevision, int maxRevision)
		{
			return null;
		}
		public virtual IEnumerable<Commit> GetFrom(DateTime start)
		{
			return null;
		}

		public virtual void Commit(Commit attempt)
		{
			var identifier = GetIdentifier(attempt);
			var version = string.Empty;

			try
			{
				version = this.storage.Put(identifier, attempt);

				// TODO: this.AddToDispatch(attempt, identifier, version)
				this.UpdateIndex(attempt, identifier, version);
				//// TODO: async update of "StreamHead"
			}
			catch (ConcurrencyException)
			{
				this.storage.Remove(identifier, version);

				throw;
			}
		}
		private static string GetIdentifier(Commit attempt)
		{
			// string format parameter will be populated with MD5 of commit.
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1 ({0}).commit

			// TODO: format path properly
			// s3://{bucket}/4/4/4/4/16/4/.../{up to 4 digits}.(commit|snapshot)
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1.snapshot
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000.snapshot
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000/0.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000/0.snapshot
			return "{0}/{1}.commit".FormatWith(attempt.StreamId, attempt.CommitSequence);
		}

		public virtual IEnumerable<Commit> GetUndispatchedCommits()
		{
			return null;
		}
		public virtual void MarkCommitAsDispatched(Commit commit)
		{
		}

		public virtual Snapshot GetSnapshot(Guid streamId, int maxRevision)
		{
			return null;
		}
		public virtual IEnumerable<StreamHead> GetStreamsToSnapshot(int maxThreshold)
		{
			return null;
		}
		public virtual bool AddSnapshot(Snapshot snapshot)
		{
			return false;
		}

		private void UpdateIndex(Commit committed, string identifier, string version)
		{
		}
	}
}