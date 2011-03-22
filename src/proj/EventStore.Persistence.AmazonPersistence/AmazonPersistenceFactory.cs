namespace EventStore.Persistence.AmazonPersistence
{
	using Amazon;
	using Amazon.S3;
	using Amazon.S3.Model;
	using Serialization;

	public class AmazonPersistenceFactory : IPersistenceFactory
	{
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
			var s3Client = AWSClientFactory.CreateAmazonS3Client(accessKey, secretKey, new AmazonS3Config
			{
				CommunicationProtocol = Protocol.HTTPS,
				MaxErrorRetry = 3,
				UseSecureStringForAwsSecretKey = true // TODO: doesn't work in medium trust
			});

			var storage = new SimpleStorageClient(s3Client, this.bucketName);

			return new AmazonPersistenceEngine(storage, this.serializer);
		}
	}
}