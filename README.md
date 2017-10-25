# s3cmdwin

Tiny command line utility to store file in AWS S3 for Windows

Usage:

`c:\Users\Batman>s3cmdwin iamUsername:iamSecretkey regionName bucketName file`

Where

* `iamUsername:iamSecretkey` - the IAM login/password
* `regionName` - the name of AWS region, for example `us-east-1`
* `bucketName` - the name of you bucket
* `file` (optional) - FULL path to the file you want to store in the bucket. If not specified - list of objects in the bucket is shown.
