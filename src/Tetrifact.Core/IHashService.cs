namespace Tetrifact.Core
{
    public interface IHashService
    {
        /// <summary>
        /// Generates a SHA256 hash of the file at the given path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string FromFile(string filePath);

        /// <summary>
        /// Generates a SHA256 hash from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string FromByteArray(byte[] data);

        /// <summary>
        /// Generates a SHA256 hash from a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string FromString(string str);

        /// <summary>
        /// Sorts file paths so they are in standard order for hash creation.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        string[] SortFileArrayForHashing(string[] files);
    }
}
