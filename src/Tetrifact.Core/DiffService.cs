using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetrifact.Core
{
    public class DiffService
    {
        private IIndexReader _indexReader;

        private IPackageList _packageList;

        public DiffService(IIndexReader indexReader, IPackageList packageList) 
        {
            _indexReader = indexReader;
            _packageList = packageList;
        }

        public void Process() 
        {
            foreach (string project in _packageList.GetProjects()) 
            {
                // processed oldest first
                IEnumerable<Package> undiffedPackages = _packageList.GetUndiffedPackages(project).OrderBy(r => r.CreatedUtc);
                foreach(Package undiffedPackage in undiffedPackages)
                    this.ProcessPackage(undiffedPackage);
            }

        }

        public void ProcessPackage(Package package) 
        {
            
            // if no head (this is first package in project), or head doesn't contain the same file path, write incoming as raw bin
            if (useFileAsBin)
            {
                StreamsHelper.FileCopy(incomingFilePath, writePath, i * _settings.FileChunkSize, ((i + 1) * _settings.FileChunkSize));
            }
            else if (linkDirect)
            {
                itemType = ManifestItemTypes.Link;
            }
            else // create patch
            {
                // create patch against head version of file
                string sourceBinPath = _indexReader. RehydrateOrResolveFile(_project, parentPackage, filePath);

                // check if upstream file has content at for this chunk point. if not, write the entire incoming file as a "bin" type
                if (new FileInfo(sourceBinPath).Length < i * _settings.FileChunkSize)
                {
                    StreamsHelper.FileCopy(incomingFilePath, writePath, i * _settings.FileChunkSize, ((i + 1) * _settings.FileChunkSize));
                }
                else
                {
                    itemType = ManifestItemTypes.Patch;

                    // write to patchPath, using incomingFilePath diffed against sourceBinPath
                    using (FileStream patchStream = new FileStream(writePath, FileMode.Create, FileAccess.Write))
                    using (FileStream binarySourceStream = new FileStream(sourceBinPath, FileMode.Open, FileAccess.Read))
                    using (MemoryStream binarySourceChunkStream = new MemoryStream())
                    {
                        // we want only a portion of the binary source file, so we copy that portion to a chunk memory stream
                        binarySourceStream.Position = i * _settings.FileChunkSize;
                        StreamsHelper.StreamCopy(binarySourceStream, binarySourceChunkStream, ((i + 1) * _settings.FileChunkSize));
                        binarySourceChunkStream.Position = 0;

                        using (FileStream incomingFileStream = new FileStream(incomingFilePath, FileMode.Open, FileAccess.Read))
                        using (MemoryStream incomingFileChunkStream = new MemoryStream())
                        {
                            // similarly, we want only a portion of the incoming file
                            incomingFileStream.Position = i * _settings.FileChunkSize;
                            StreamsHelper.StreamCopy(incomingFileStream, incomingFileChunkStream, ((i + 1) * _settings.FileChunkSize));
                            incomingFileChunkStream.Position = 0;

                            // if incoming stream is empty, we'll jump over this and end up with an empty patch
                            if (incomingFileChunkStream.Length >= 0)
                            {
                                VCCoder coder = new VCCoder(binarySourceChunkStream, incomingFileChunkStream, patchStream);
                                VCDiffResult result = coder.Encode(); //encodes with no checksum and not interleaved
                                if (result != VCDiffResult.SUCCESS)
                                {
                                    string error = $"Error patching incoming file {sourceBinPath} against source {incomingFilePath}.";
                                    Console.WriteLine(error);
                                    throw new Exception(error);
                                }
                            }
                        }
                    }
                }
            }
                    
        }
    }
}
