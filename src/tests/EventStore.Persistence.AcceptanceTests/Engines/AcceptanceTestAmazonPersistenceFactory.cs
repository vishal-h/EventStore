namespace EventStore.Persistence.AcceptanceTests.Engines
{
	using System.Configuration;
	using AmazonPersistence;
	using Serialization;

	public class AcceptanceTestAmazonPersistenceFactory : AmazonPersistenceFactory
	{
		private static readonly string BucketName = ConfigurationManager.AppSettings["AmazonBucketName"];
		private static readonly string AccessKey = ConfigurationManager.AppSettings["AmazonAccessKey"];
		private static readonly string SecretKey = ConfigurationManager.AppSettings["AmazonSecretKey"];

		public AcceptanceTestAmazonPersistenceFactory()
			: base(BucketName, AccessKey, SecretKey, new BinarySerializer())
		{
		}
	}
}