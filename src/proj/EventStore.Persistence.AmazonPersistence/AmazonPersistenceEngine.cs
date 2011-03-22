namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using Serialization;

	public class AmazonPersistenceEngine : IPersistStreams
	{
		private readonly SimpleStorageClient storage;
		private readonly ISerialize serializer;
		private int initialized;

		public AmazonPersistenceEngine(SimpleStorageClient storage, ISerialize serializer)
		{
			this.storage = storage;
			this.serializer = serializer;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
		}

		public virtual void Initialize()
		{
			if (Interlocked.Increment(ref this.initialized) > 1)
				return;

			// assert that the bucket exists?
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

			try
			{
				this.PutCommit(attempt, identifier);
				this.UpdateIndex(attempt, identifier);
			}
			catch (ConcurrencyException)
			{
				var commit = this.GetCommit(identifier);
				this.UpdateIndex(commit, identifier);
			}
		}
		private static string GetIdentifier(Commit attempt)
		{
			// s3://{bucket}/4/4/4/4/16/4/.../{up to 4 digits}.(commit|snapshot)
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1.snapshot
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000.snapshot
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000/0.commit
			// s3://mybucket/1ed1/d4bd/a01c/4bc6/ab4cee9720eafce5/1000/0.snapshot

			// TODO: format path properly
			return "{0}/{1}.commit".FormatWith(attempt.StreamId, attempt.CommitSequence);
		}
		private void PutCommit(Commit attempt, string identifier)
		{
			using (var stream = new MemoryStream())
			{
				this.serializer.Serialize(stream, attempt);
				stream.Position = 0;
				this.storage.Put(identifier, stream);
			}
		}
		private void UpdateIndex(Commit committed, string identifier)
		{
			// this should never throw a "duplicate" exception
			// this should be async
		}
		private Commit GetCommit(string identifier)
		{
			using (var stream = this.storage.Get(identifier))
				return this.serializer.Deserialize<Commit>(stream);
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
	}
}