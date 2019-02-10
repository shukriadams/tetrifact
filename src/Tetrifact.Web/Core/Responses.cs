using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Web
{
    public class Responses
    {
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
                    Message = string.Format("A package with id {0} already exists.", packageId)
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
                    Message = string.Format("Invalid package content. A package flagged as archive must contain a single zip file.")
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
                    Message = string.Format("The package format {0} is not valid. Valid values are : zip", format)
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
                    Message = string.Format("The package upload failed. Please check log enry {0} for more information.", logId)
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
                    Message = string.Format("The file id you gave is not valid. File ids should be obtained from manifests. ")
                }
            });
        }

    }
}
