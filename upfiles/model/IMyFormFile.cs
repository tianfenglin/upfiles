using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace upfiles.model
{
    public interface IMyFormFile : IFormFileCollection
    {
        string FileMd5 { get; }
        string Chunk { get; }
        string ChunkSize { get; }
        string FilePath { get; }
        string FileName { get; }
    }
}
