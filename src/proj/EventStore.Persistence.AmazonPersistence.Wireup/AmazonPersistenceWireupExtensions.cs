namespace EventStore
{
	using Persistence.AmazonPersistence;
	using Serialization;

	public static class MongoPersistenceWireupExtensions
	{
		public static Wireup UsingAmazonPersistence(
			this Wireup wireup, string connectionName, ISerialize serializer)
		{
			// TODO: connection info
			wireup.With(new AmazonPersistenceFactory(null, null, null, serializer).Build());
			return wireup;
		}
	}
}