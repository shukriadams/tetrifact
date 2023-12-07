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
                statusCode = 404,
                error = new
                {
                    description = description
                }
            });
        }

        public static BadRequestObjectResult InvalidJSONError()
        {
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
                error = new
                {
                    code = 100,
                    description = $"JSON invalid"
                }
            });
        }

        public static BadRequestObjectResult PackageAlreadyExistsError(Controller controller, string packageId)
        {
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
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
                statusCode = 400,
                error = new
                {
                    code = 101,
                    description = "An internal error occurred. Please check server logs for more info."
                }
            });
        }

        public static BadRequestObjectResult UnexpectedError(string message)
        {
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
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
                statusCode = 400,
                error = new
                {
                    code = 102,
                    description = "Invalid package content. A package flagged as archive must contain a single zip file."
                }
            });
        }

        public static BadRequestObjectResult GeneralUserError(string description)
        {
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
                error = new
                {
                    code = 101,
                    description = description
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
                statusCode = 400,
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
        /// <returns></returns>
        public static BadRequestObjectResult InvalidFileId()
        {
            return new BadRequestObjectResult(new
            {
                statusCode = 400,
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
                statusCode = 400,
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
                statusCode = 400,
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
