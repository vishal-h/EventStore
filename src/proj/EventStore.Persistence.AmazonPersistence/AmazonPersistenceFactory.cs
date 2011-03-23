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
			var storage = new SimpleStorageClient(this.BuildAmazonS3Client(), this.bucketName, this.serializer);
			return new AmazonPersistenceEngine(storage);
		}
		private AmazonS3 BuildAmazonS3Client()
		{
			var config = new AmazonS3Config()
				.WithCommunicationProtocol(Protocol.HTTPS)
				.WithMaxErrorRetry(3)
				.WithUseSecureStringForAwsSecretKey(true); // TODO: make configurable; doesn't work in medium trust

			return AWSClientFactory.CreateAmazonS3Client(this.accessKey, this.secretKey, config);
		}
	}
}