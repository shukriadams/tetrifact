using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class Responses
    {
        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static BadRequestObjectResult PackageExistsError(string packageId)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 303,
                    Message = $"A package with id {packageId} already exists."
                }
            });
        }

        public static BadRequestObjectResult UnexpectedError()
        {
            return UnexpectedError(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicError">Optional message which may be exposed</param>
        /// <returns></returns>
        public static BadRequestObjectResult UnexpectedError(string publicError)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.01,
                    Message = $"An internal server occurred. You didn't do anything wrong. {publicError}"  
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static BadRequestObjectResult InvalidArchiveContent()
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.02,
                    Message = "Invalid package content. A package flagged as archive must contain a single zip file."
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static BadRequestObjectResult InvalidArchiveFormatError(string format)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.03,
                    Message = $"The package format {format} is not valid. Valid values are : zip" 
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logId"></param>
        /// <returns></returns>
        public static BadRequestObjectResult PackageUploadFailedError(string logId)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.04,
                    Message = $"The package upload failed. Please check log enry {logId} for more information."
                }
            });
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static BadRequestObjectResult InvalidFileId()
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.05,
                    Message = "The file id you gave is not valid. File ids should be obtained from manifests."
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static BadRequestObjectResult MissingInputError(string error)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.06,
                    Message = error
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static BadRequestObjectResult InsufficientSpace(string error)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.07,
                    Message = error
                }
            });
        }

        public static BadRequestObjectResult InvalidCharacters(string message)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 400.08,
                    Message = message
                }
            });
        }

        #endregion
    }
}
