using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LudaFit.SharedKernel.Interfaces;

namespace LudaFit.Infrastructure.AzureBlobStorage;

//todo: поменять заметку про блоб
public sealed class BlobRepository(BlobServiceClient blobServiceClient) : IScopedType
{
    public async Task<string> AddFileAndGetUrlAsync(AzureBlobContainerName containerName, string fileName, Stream fileStream, CancellationToken ct)
    {
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        BlobClient blobClient = container.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }

    public Uri GetFileUrl(string containerName, string fileName)
    {
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blob = container.GetBlobClient(fileName);
        return blob.Uri;
    }

    public async Task DeleteFileAsync(AzureBlobContainerName containerName, string fileName, CancellationToken ct)
    {
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = container.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task DeleteAllFilesByNameAsync(AzureBlobContainerName containerName, string fileName, CancellationToken ct)
    {
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);

        GetBlobsOptions options = new()
        {
            Prefix = fileName
        };
        
        await foreach (BlobItem blobItem in container.GetBlobsAsync(options, ct))
        {
            string blobNameWithoutExtension = Path.GetFileNameWithoutExtension(blobItem.Name);

            if (blobNameWithoutExtension != fileName)
            {
                continue;
            }

            BlobClient blobClient = container.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
    }
}
