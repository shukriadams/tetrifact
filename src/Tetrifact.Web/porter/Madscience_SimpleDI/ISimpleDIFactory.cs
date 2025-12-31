namespace Tetrifact.Web.Porter_Packages {

using System;
    
namespace MadScience_SimpleDI
{
    /// <summary>
    /// If you want to resolve your own types at calling time, implement this interface on some class and bind
    /// it against the type it will provide.
    /// </summary>
    public interface ISimpleDIFactory
    {
        object Resolve<T>();

        object Resolve(Type service);
    }
}

}