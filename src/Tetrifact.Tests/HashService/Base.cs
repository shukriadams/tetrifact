namespace Tetrifact.Tests.HashService
{
    public abstract class Base : FileSystemBase
    {
        protected readonly string _input = "test input";

        /// <summary>
        /// Known SHA256 output from the above input string
        /// </summary>
        protected readonly string _expectedHash = "9dfe6f15d1ab73af898739394fd22fd72a03db01834582f24bb2e1c66c7aaeae";
    }
}
