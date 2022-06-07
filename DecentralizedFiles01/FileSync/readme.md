This library provides file data broker functionality between local storage and 
Filebase's S3 interface.

The main artefact will be stuctured json file containing a social media post as well as pointers
to other media files. 

Both posts and media files will be exposed to the outside world via a "Pre-Signed URL" with
an explicit duration.

Note: The FileBaseDirectoryWatcher Program is a simple proof-of-concept app; and, 
For now Each developer must supply their own secrets.json file with this structure. 
Remember the secrets.json need to mirror the appsettings.json.

appsettings.json-
{
  "FileBaseDirectory": "E:\\Archive\\social-media",
  "Mode": "Dev",
  "S3BucketName": "someBucket",
  "FilebaseCredentials": {
    "AccessKey": "SomeKey",
    "SecretKey": "SomeSecret"
  }
}

secrets.json-
{
  "S3BucketName": "SuperSecretBucketName",
  "FilebaseCredentials": {
    "AccessKey": "SuperSecretKey",
    "SecretKey": "SuperSecretSecret"
  }
}