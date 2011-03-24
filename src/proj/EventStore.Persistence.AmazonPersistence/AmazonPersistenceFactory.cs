namespace EventStore.Persistence.AmazonPersistence
{
	using Amazon;
	using Amazon.S3;
	using Amazon.S3.Model;
	using Amazon.SimpleDB;
	using Serialization;

	public class AmazonPersistenceFactory : IPersistenceFactory
	{
		private const int MaxRetries = 3;
		private const bool SecureSecretKey = true; // TODO: make configurable; doesn't work in medium trust
		private readonly string bucketName;
		private readonly string accessKey;
		private readonly string secretKey;
		private readonly ISerialize serializer;

		public AmazonPersistenceFactory(string bucketName, string accessKey, string secretKey, ISerialize serializer)
		{
			this.bucketName = bucketName;
			this.accessKey = accessKey;
			this.secretKey = secretKey;
			this.serializer = serializer;
		}

		public virtual IPersistStreams Build()
		{
			var database = new SimpleDbClient(this.BuildSimpleDbClient());
			var storage = new SimpleStorageClient(this.BuildAmazonS3Client(), this.bucketName, this.serializer);
			return new AmazonPersistenceEngine(database, storage);
		}
		private AmazonSimpleDB BuildSimpleDbClient()
		{
			var config = new AmazonSimpleDBConfig()
				.WithMaxErrorRetry(MaxRetries)
				.WithUseSecureStringForAwsSecretKey(SecureSecretKey);

			return AWSClientFactory.CreateAmazonSimpleDBClient(this.accessKey, this.secretKey, config);
		}
		private AmazonS3 BuildAmazonS3Client()
		{
			var config = new AmazonS3Config()
				.WithCommunicationProtocol(Protocol.HTTPS)
				.WithMaxErrorRetry(MaxRetries)
				.WithUseSecureStringForAwsSecretKey(SecureSecretKey);

			return AWSClientFactory.CreateAmazonS3Client(this.accessKey, this.secretKey, config);
		}
	}
}