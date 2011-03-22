namespace EventStore.Persistence.AmazonPersistence
{
	using System;
	using System.IO;
	using Amazon.S3;
	using Amazon.S3.Model;

	public class SimpleStorageClient : IDisposable
	{
		private const int MaxAttempts = 3;
		private static readonly int Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
		private readonly AmazonS3 client;
		private readonly string bucketName;

		public SimpleStorageClient(AmazonS3 client, string bucketName)
		{
			this.client = client;
			this.bucketName = bucketName;
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

		public virtual bool Put(string identifier, Stream input)
		{
			// TODO: object already exists? get the object and determine if it's duplicate or not...
			var request = new PutObjectRequest
			{
				BucketName = this.bucketName,
				Key = identifier,
				GenerateMD5Digest = true,
				InputStream = input,
				Timeout = Timeout,
			};

			return Try(() =>
			{
				using (this.client.PutObject(request))
					return true;
			});
		}
		public virtual Stream Get(string identifier)
		{
			// TODO: not found, return null
			var request = new GetObjectRequest
			{
				BucketName = this.bucketName,
				Key = identifier,
				Timeout = Timeout
			};

			return Try(() => this.client.GetObject(request).ResponseStream);
		}

		protected virtual T Try<T>(Func<T> callback)
		{
			return Try(callback, 0);
		}
		private static T Try<T>(Func<T> callback, int attempt)
		{
			try
			{
				return callback();
			}
			catch (TimeoutException e)
			{
				if (attempt >= MaxAttempts)
					return default(T);

				throw new StorageUnavailableException(e.Message, e);
			}
			catch (AmazonS3Exception e)
			{
				throw new StorageException(e.Message, e);
			}
		}
	}
}