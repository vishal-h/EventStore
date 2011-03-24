namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.Collections.Generic;
	using Amazon.SimpleDB;
	using Amazon.SimpleDB.Model;

	public class SimpleDbClient
	{
		private const string PrimaryKey = "{0}/{1}";
		private const string PayloadIdentifier = "{0}@{1}";
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
				.WithDomainName("Commits") // TODO: sharding
				.WithItemName(PrimaryKey.FormatWith(attempt.StreamId, attempt.CommitSequence))
				.WithExpected(optimisticConcurrency)
				.WithAttribute(GetAttributes(attempt, identifier, version));

			// TODO: try/catch
			this.client.PutAttributes(request);
			return true;
		}
		private static ReplaceableAttribute[] GetAttributes(Commit attempt, string identifier, string version)
		{
			return new[]
			{
				new ReplaceableAttribute()
					.WithName("MinStreamRevision")
					.WithValue((attempt.StreamRevision - attempt.Events.Count).ToString())
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("MaxStreamRevision")
					.WithValue(attempt.StreamRevision.ToString())
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("CommitId")
					.WithValue(attempt.CommitId.ToString())
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("CommitSequence")
					.WithValue(attempt.CommitSequence.ToString())
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("CommitStamp")
					.WithValue(attempt.CommitStamp.ToString())
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("Dispatched")
					.WithValue("false")
					.WithReplace(false),

				new ReplaceableAttribute()
					.WithName("Payload")
					.WithValue(PayloadIdentifier.FormatWith(identifier, version))
					.WithReplace(false),
			};
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