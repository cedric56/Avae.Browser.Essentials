using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    partial class ShareImplementation : IShare
    {
        Task PlatformRequestAsync(ShareTextRequest request)
        {
            var script = $@"function shareContent(text, title, url) {{
                    if (navigator.canShare) {{
                        navigator.share({{
                            title: title,
                            text: text,
                            url: url
                        }}).catch((error) => console.log('Error sharing:', error));
                    }} else {{
                        alert('Sharing is not supported on this browser.');
                    }}
                }}
                shareContent('{request.Text}', '{request.Title}', '{request.Uri}')";
            JSInterop.Eval(script);
            return Task.CompletedTask;
        }

        Task PlatformRequestAsync(ShareFileRequest request) =>
            PlatformRequestAsync((ShareMultipleFilesRequest)request);

        private class File64
        {
            public string? FileName { get; set; }
            public string? FileContentBase64 { get; set; }
            public string? MimeType { get; set; }
        }

        private static async Task<byte[]> ReadFully(Stream input)
        {
            const int DefaultBufferSize = 64 * 1024; // 64 KB is usually optimal
            using var ms = new MemoryStream((int)(input.CanSeek ? input.Length : DefaultBufferSize));
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }

        async Task PlatformRequestAsync(ShareMultipleFilesRequest request)
        {
            var files64 = new List<File64>();
            foreach(var file in request.Files ?? [])
            {
                if (file.Data is null)
                {
                    using var stream = await file.OpenReadAsync();
                    if (stream is MemoryStream ms)
                    {
                        file.Data = ms.ToArray();
                    }
                    else
                    {
                        file.Data = await ReadFully(stream);
                    }
                }
                var file64 = new File64()
                {
                    FileContentBase64 = Convert.ToBase64String(file.Data),
                    FileName = file.FileName,
                    MimeType = file.ContentType ?? "application/octet-stream"
                };
                files64.Add(file64);
            }

            // Generate JavaScript code to convert each file and invoke navigator.share with multiple files
            var jsCode = @$"
    async function shareFiles() {{
        try {{
            const files = [
                {string.Join(",", files64.Select(f => $@"
                    new File(
                        [new Uint8Array(atob('{f.FileContentBase64}').split('').map(c => c.charCodeAt(0)))],
                        '{f.FileName}',
                        {{ type: '{f.MimeType}' }}
                    )"))}
            ];

            if (navigator.canShare({{ files }})) {{
                await navigator.share({{
                    title: '{request.Title}',
                    files: files
                }});
                return true;
            }} else {{
                alert('Sharing not supported on this browser.');
                return false;
            }}
        }} catch (error) {{
            return false;
        }}
    }}
    shareFiles();
";
            JSInterop.Eval(jsCode);
        }
    }
}
