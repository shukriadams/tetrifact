using System;
using System.IO;
using System.Linq;
using VCDiff.Decoders;
using VCDiff.Includes;

namespace Tetrifact.Core
{
    public class Rehydrator : IRehydrator
    {
        private IIndexReader _indexReader;

        private string _workfile;

        public Rehydrator(IIndexReader indexReader) 
        {
            _indexReader = indexReader;
        }

        public string RehydrateOrResolveFile(string project, string packageId, string filePath)
        {
            try
            {
                string projectPath = PathHelper.GetExpectedProjectPath(project);
                string shardGuid = PathHelper.GetLatestShardAbsolutePath(_indexReader, project, packageId);
                string dataPathBase = Path.Combine(projectPath, Constants.ShardsFragment, shardGuid, filePath);
                Package package = _indexReader.GetPackage(project, packageId);
                string rehydrateOutputPath = Path.Combine(Settings.TempBinaries, Obfuscator.Cloak(project), package.UniqueId.ToString(), filePath, "bin");

                PackageItem manifestItem = package.Files.FirstOrDefault(r => r.Path == filePath);
                // if neither patch nor bin exist, file doesn't exist
                if (manifestItem == null)
                    return null;

                // file has already been rehydrated by a previous process and is ready to serve
                if (File.Exists(rehydrateOutputPath))
                    return rehydrateOutputPath;

                _workfile = Path.Combine(Settings.TempBinaries, $"{Guid.NewGuid()}_bin");

                for (int i = 0; i < manifestItem.Chunks.Count; i++)
                {
                    PackageItemChunk chunk = manifestItem.Chunks[i];

                    if (chunk.Type == PackageItemTypes.Bin)
                    {
                        using (FileStream writeStream = new FileStream(_workfile, FileMode.OpenOrCreate, FileAccess.Write))
                        using (FileStream readStream = new FileStream(Path.Combine(dataPathBase, $"chunk_{i}"), FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            writeStream.Position = writeStream.Length; // always append to end of this stream
                            StreamsHelper.StreamCopy(readStream, writeStream, readStream.Length);
                        }
                    }
                    else if (chunk.Type == PackageItemTypes.Link)
                    {
                        // read chunk link from source
                        string binarySourcePath = RehydrateOrResolveFile(project, package.Parent, filePath);

                        using (FileStream writeStream = new FileStream(_workfile, FileMode.OpenOrCreate, FileAccess.Write))
                        using (FileStream readStream = new FileStream(Path.Combine(binarySourcePath), FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            readStream.Position = i * package.FileChunkSize;
                            writeStream.Position = writeStream.Length; // always append to end of this stream
                            StreamsHelper.StreamCopy(readStream, writeStream, (i + 1) * package.FileChunkSize);
                        }
                    }
                    else
                    {
                        // read source chunk against self patch
                        string binarySourcePath = RehydrateOrResolveFile(project, package.Parent, filePath);

                        using (FileStream writeStream = new FileStream(_workfile, FileMode.OpenOrCreate, FileAccess.Write))
                        using (FileStream binarySourceStream = new FileStream(Path.Combine(binarySourcePath), FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (FileStream patchStream = new FileStream(Path.Combine(dataPathBase, $"chunk_{i}"), FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (MemoryStream binarySourceChunkStream = new MemoryStream())
                        {
                            // we want only a portion of the binary source file, so we copy that portion to a chunk memory stream
                            binarySourceStream.Position = i * package.FileChunkSize;
                            StreamsHelper.StreamCopy(binarySourceStream, binarySourceChunkStream, ((i + 1) * package.FileChunkSize));
                            binarySourceChunkStream.Position = 0;

                            writeStream.Position = writeStream.Length; // always append to end of this stream

                            // if patch is empty, write an empty output file
                            if (patchStream.Length > 0)
                            {
                                VCDecoder decoder = new VCDecoder(binarySourceChunkStream, patchStream, writeStream);

                                // You must call decoder.Start() first. The header of the delta file must be available before calling decoder.Start()

                                VCDiffResult result = decoder.Start();

                                if (result != VCDiffResult.SUCCESS)
                                {
                                    //error abort
                                    throw new Exception($"vcdiff abort error in file {filePath}");
                                }

                                long bytesWritten = 0;
                                result = decoder.Decode(out bytesWritten);

                                if (result != VCDiffResult.SUCCESS)
                                {
                                    //error decoding
                                    throw new Exception($"vcdiff decode error in file {filePath}");
                                }
                            }
                        }
                    }
                }

                FileHelper.EnsureParentDirectoryExists(rehydrateOutputPath);

                // check target location again 
                if (File.Exists(rehydrateOutputPath))
                    return rehydrateOutputPath;

                try
                {
                    File.Move(_workfile, rehydrateOutputPath);
                }
                catch (IOException) 
                {
                    // ignore these, race condition has occurred and another process
                    // beat us to the file
                }

                return rehydrateOutputPath;
            }
            finally 
            {
                // clean up work file
                if (_workfile != null)
                    File.Delete(_workfile);
            }

        }
    }
}
