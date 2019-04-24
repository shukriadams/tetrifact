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
                    code = 100,
                    Message = $"A package with id {packageId} already exists."
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
                    Message = "An internal server occurred. You didn't do anything wrong."
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
                    code = 103,
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
                    code = 104,
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
                    code = 105,
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
                    code = 106,
                    Message = error
                }
            });
        }

        #endregion
    }
}
