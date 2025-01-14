﻿using GrillBot.Common.Services.Common;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.FileService;

public interface IFileServiceClient : IClient
{
    Task<DiagnosticInfo> GetDiagAsync();
    Task UploadFileAsync(string filename, byte[] content, string contentType);
    Task<byte[]?> DownloadFileAsync(string filename);
    Task DeleteFileAsync(string filename);
    Task<string?> GenerateLinkAsync(string filename);
}
