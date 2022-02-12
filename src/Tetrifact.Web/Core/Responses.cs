using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class Responses
    {
        #region METHODS
    
        /// <summary>
        /// Generic 404 JSON response
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static NotFoundObjectResult NotFoundError(Controller controller, string description)
        {
            return controller.NotFound(new
            {
                error = new
                {
                    description = description
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static NotFoundObjectResult PackageNotExistError(Controller controller, string packageId)
        {
            return controller.NotFound(new
            {
                error = new
                {
                    code = 100,
                    description = $"A package with id {packageId} already exists."
                }
            });
        }

        public static BadRequestObjectResult UnexpectedError()
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 101,
                    description = "An internal server occurred. You didn't do anything wrong."
                }
            });
        }

        public static BadRequestObjectResult UnexpectedError(string message)
        {
            return new BadRequestObjectResult(new
            {
                error = new
                {
                    code = 101,
                    description = message
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
                    code = 102,
                    description = "Invalid package content. A package flagged as archive must contain a single zip file."
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
                    code = 103,
                    description = $"The package format {format} is not valid. Valid values are : zip" 
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
                    code = 104,
                    description = $"The package upload failed. Please check log enry {logId} for more information."
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
                    code = 105,
                    description = "The file id you gave is not valid. File ids should be obtained from manifests."
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
                    code = 106,
                    description = error
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
                    code = 107,
                    description = error
                }
            });
        }

        #endregion
    }
}
