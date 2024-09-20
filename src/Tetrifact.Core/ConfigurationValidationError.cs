using System;

namespace Tetrifact.Core
{
    public class ConfigurationValidationError
    {
        public bool IsValid { get; set; }

        public string Message { get; set; }

        public Exception InnerException { get; set; }

        public ConfigurationValidationError()
        {
            this.Message = string.Empty;
        }
    }
}