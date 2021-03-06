﻿using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3cmdwin
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 3 || args[0]=="/?")
			{
				Console.WriteLine(@"Usage:

s3cmdwin iamUsername:iamSecretkey regionName bucketName file

Where

""iamUsername:iamSecretkey"" - the IAM login/password
""regionName"" - the name of AWS region, for example ""us-east-1""
""bucketName"" - the name of you bucket
""file"" (optional) - FULL path to the file you want to store in the bucket. If not specified - list of objects in the bucket is shown.");
				return;
			}

			try
			{
				string iamLogin = args[0].Split(':')[0];
				string iamPassword = args[0].Split(':')[1];
				string regionName = args[1];
				string bucketName = args[2];

				var credentials = new Amazon.Runtime.BasicAWSCredentials(iamLogin, iamPassword);
				var config = new AmazonS3Config() { UseHttp = true, RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(regionName) };
				var client = new AmazonS3Client(credentials, config);

				if (args.Length > 3) //store file
				{
					string filename = args[3];

					//limit to 1 concurrent request to ease on CPU with big files
					//also set chunksize to 5GB (max allowed) to lower S3 costs, otherwise it splits into too many requests, a 20GB file will be THOUSANDS requests
					var transferConfig = new Amazon.S3.Transfer.TransferUtilityConfig()
					{
						ConcurrentServiceRequests = 1, //limit to 1 concurrent request to ease on CPU with big files
						MinSizeBeforePartUpload = 5000000000 //5GB. yes i know 5GB is actually 5368709120, just in case SDK packs more reserved info into the PUT request (headers, etc)
					};
					var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(client, transferConfig);
					fileTransferUtility.Upload(filename, bucketName);
					Console.WriteLine($"Upload completed");
				}
				else //list files in the bucket
				{
					ListObjects(client, bucketName);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static void ListObjects(AmazonS3Client client, string bucketName)
		{
			var request = new Amazon.S3.Model.ListObjectsV2Request
			{
				BucketName = bucketName,
				MaxKeys = 50
			};
			Amazon.S3.Model.ListObjectsV2Response response;
			do
			{
				response = client.ListObjectsV2(request);

				// Process response.
				foreach (var entry in response.S3Objects)
				{
					Console.WriteLine("key = {0} size = {1}",
						entry.Key, entry.Size);
				}
				request.ContinuationToken = response.NextContinuationToken;
			} while (response.IsTruncated == true);
		}
	}
}
