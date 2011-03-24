namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.IO;
	using Amazon.S3;
	using Amazon.S3.Model;
	using Serialization;

	public class SimpleStorageClient : IDisposable
	{
		private static readonly int Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
		private readonly AmazonS3 client;
		private readonly string bucketName;
		private readonly ISerialize serializer;

		public SimpleStorageClient(AmazonS3 client, string bucketName, ISerialize serializer)
		{
			this.client = client;
			this.bucketName = bucketName;
			this.serializer = serializer;
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
			// TODO: create bucket if it doesn't already exist
		}

		public virtual string Put<T>(string identifier, T item)
		{
			using (var stream = new MemoryStream())
			{
				this.serializer.Serialize(stream, item);
				stream.Position = 0;
				return this.PutStream(identifier, stream);
			}
		}
		private string PutStream(string identifier, Stream input)
		{
			var request = new PutObjectRequest()
				.WithBucketName(this.bucketName)
				.WithTimeout(Timeout)
				.WithKey(identifier)
				.WithGenerateChecksum(true)
				.WithInputStream(input) as PutObjectRequest;

			return this.Try(() =>
			{
				using (var response = this.client.PutObject(request))
					return response.VersionId;
			});
		}
		
		public virtual T Get<T>(string identifier, string version)
		{
			using (var stream = this.GetStream(identifier, version))
				return this.serializer.Deserialize<T>(stream);
		}
		private Stream GetStream(string identifier, string version)
		{
			var request = new GetObjectRequest()
				.WithBucketName(this.bucketName)
				.WithKey(identifier)
				.WithVersionId(version)
				.WithTimeout(Timeout);

			return this.Try(() => this.client.GetObject(request).ResponseStream);
		}

		public virtual void Remove(string identifier, string version)
		{
			var request = new DeleteObjectRequest()
				.WithBucketName(this.bucketName)
				.WithKey(identifier)
				.WithVersionId(version);

			this.Try(() => this.client.BeginDeleteObject(request, state => { }, null));
		}

		protected virtual T Try<T>(Func<T> callback)
		{
			try
			{
				return callback();
			}
			catch (TimeoutException e)
			{
				throw new StorageUnavailableException(e.Message, e);
			}
			catch (AmazonS3Exception e)
			{
				throw new StorageException(e.Message, e);
			}
		}
	}
}