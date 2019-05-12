using System;

namespace Tetrifact.Core
{
    public class InvalidFileIdentifierException : Exception
    {
        public InvalidFileIdentifierException(string id): base($"{id} is not a valid identifier. Identifier format is base64 encoded path::hash.")
        {

        }
    }
}
