# BlobStandard

High-performance storage abstraction layer; the .NET equivalent of Rust's [`object_store` crate](https://crates.io/crates/object_store).

## Goals

* Allow code to be written once, and support multiple storage backends.
* Provide high performance using modern .NET APIs, such as `System.IO.Pipelines`.
* Provide a common denominator of functionality that is efficiently supported across all backends, while allowing users to take advantage of backend-specific features when needed.
* Integrate with .NET's dependency injection and configuration facilities.

## Supported Backends

> [!IMPORTANT]
> Not all backends are currently implemented; this is a list of planned backends.

* Local files
* [Amazon S3](https://aws.amazon.com/s3/)
* [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs)
* [Azure Data Lake Storage (Gen2)](https://azure.microsoft.com/en-us/products/storage/data-lake-storage)
* [Google Cloud Storage](https://cloud.google.com/storage)
* Custom implementations

## Supported operations

* Download blob
  * Supports byte-range fetching
* Upload blob sequentially
  * Uploading a blob replaces any existing blob with the same name, but it is possible to upload it if it does not already exist.
  * Uses multipart uploads (or equivalents) for large uploads
  * Supports serializing the state of an upload, allowing it to be resumed later
* Upload blob in parallel
  * Allows users control multipart uploads
* List blobs under prefix
  * Supports both flat and hierarchical listing
* Delete blob

### Potential future additions

* Custom blob metadata
  * Initially, only `Content-Type` will be supported.
* Generating presigned URLs
* More complex conditional operations, besides "upload if blob does not already exist".

## Comparison to existing libraries

> [!NOTE]
> This subjective comparison was based on a brief review of each library, and may not be entirely accurate. Please open an issue or submit a PR if you think any of the information is incorrect.

* [FileParty](https://github.com/JankwareDotCom/FileParty)
  * More high-level than BlobStandard, does not use `System.IO.Pipelines`, does not support byte-range fetching.
* [FluentStorage](https://github.com/robinrodricks/FluentStorage)
  * More high-level than BlobStandard, supports more than object storage, does not use `System.IO.Pipelines`, and some of its features are [prone to race conditions](https://github.com/robinrodricks/FluentStorage/blob/4e077189e27086f64798eef2a03ddce942569cc5/FluentStorage.AWS/Blobs/Converter.cs#L39-L46).
* [Stowage](https://github.com/aloneguid/stowage)
  * Its goal to not depend on the cloud provider SDKs is a bad idea, because they handle _a ton_ of complexity under the hood.

## License

BlobStandard is licensed under the [MIT license](https://opensource.org/licenses/MIT).

## Maintainers

* [__@teo-tsirpanis__](https://github.com/teo-tsirpanis)
