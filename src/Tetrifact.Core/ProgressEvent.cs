namespace Tetrifact
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta">Amount of work done towards to total</param>
    /// <param name="total">Total amount of work to be done.</param>
    public delegate void ProgressEvent(long delta, long total);
}
