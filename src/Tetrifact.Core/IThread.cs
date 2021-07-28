namespace Tetrifact.Core
{

    /// <summary>
    /// Wraps System.Thread calls to make it easier to unit test them
    /// </summary>
    public interface IThread
    {
        public void Sleep(int ms);
    }
}
