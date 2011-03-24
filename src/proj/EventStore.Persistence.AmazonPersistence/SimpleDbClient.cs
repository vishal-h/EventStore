namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.Collections.Generic;
	using Amazon.SimpleDB;
	using Amazon.SimpleDB.Model;

	public class SimpleDbClient
	{
		private const string ItemName = "{0}/{1}";
		private const string RevisionQuery = "select * from {0} where StreamId = {1} and MinRevision >= {2} and MaxRevision <= {3} order by MinRevision";
		private readonly AmazonSimpleDB client;

		public SimpleDbClient(AmazonSimpleDB client)
		{
			this.client = client;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				this.client.Dispose();
		}

		public virtual void Initialize()
		{
		}

		public virtual bool PutCommit(Commit attempt, string identifier, string version)
		{
			var optimisticConcurrency = new UpdateCondition()
				.WithExists(false);

			var request = new PutAttributesRequest()
				.WithDomainName("Commits") // TODO: sharding?
				.WithItemName(ItemName.FormatWith(attempt.StreamId, attempt.CommitSequence))
				.WithExpected(optimisticConcurrency)
				.WithAttribute(null);

			return true;
		}

		public virtual IEnumerable<Commit> Select(string domain, string streamId, int minRevision, int maxRevision)
		{
			var request = new SelectRequest
			{
				ConsistentRead = false,
				SelectExpression = RevisionQuery.FormatWith(domain, streamId, minRevision, maxRevision)
			};

			// TODO: as a future optimization, we should consider using a multi-threaded producer/consumer
			// so that the client isn't waiting while results are being streamed.
			return null;
		}
	}
}