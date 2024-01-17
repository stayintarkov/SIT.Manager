// Source: https://gist.github.com/dalexsoto/9fd3c5bdbe9f61a717d47c5843384d11

namespace SIT.Manager.Updater.Classes
{
    public static class HttpClientProgressExtensions
    {
        public static async Task DownloadDataAsync(this HttpClient client, string requestUrl, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using HttpResponseMessage response = await client.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            long? contentLength = response.Content.Headers.ContentLength;
            using var download = await response.Content.ReadAsStreamAsync(cancellationToken);
            // no progress... no contentLength... very sad
            if (progress is null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
                return;
            }
            // Such progress and contentLength much reporting Wow!
            var progressWrapper = new Progress<long>(totalBytes => progress.Report(((float)totalBytes / contentLength.Value)));
            await download.CopyToAsync(destination, 81920, progressWrapper, cancellationToken);
        }

        static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);
            if (!source.CanRead)
                throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
            if (!destination.CanWrite)
                throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

            byte[] buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
