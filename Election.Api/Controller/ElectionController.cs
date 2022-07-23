using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Election.Controller.Model;
using Election.Domain.Model;
using Election.Exceptions;
using Election.Helper;
using Election.Middleware;
using Election.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Election.Controller
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ImageHelper _imageHelper;

        public ElectionController(IApplicationService applicationService, ImageHelper imageHelper)
        {
            _applicationService = applicationService;
            _imageHelper = imageHelper;
        }

        /// <summary>
        /// Get Election Info
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getElectionInfo
        ///     
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="200">Returns ok</response>
        /// <response code="400">If the activityName is null or empty</response>
        [HttpGet("/{name}/get")]
        [ProducesResponseType(typeof(string),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetElectionInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(new ApiError(new ApiException.ValueCannotBeNullOrEmptyException(nameof(name))));
            ElectionInfo election = _applicationService.GetElectionInfo(name).GetAwaiter().GetResult();
            if (election == null) return NotFound();
            return Ok(election);
        }
        
        /// <summary>
        /// Get Election Info
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /getElectionInfo
        ///     
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="200">Returns ok</response>
        /// <response code="400">If the activityName is null or empty</response>
        [HttpGet("/getAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            List<ElectionInfo> elections = _applicationService.GetAllElections().GetAwaiter().GetResult();
            if (elections == null || elections.Count == 0) return NotFound();
            return Ok(elections);
        }
        
        /// <summary>
        /// Create New Election Info
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /getElectionInfo
        ///     
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="201">Returns created</response>
        /// <response code="400">If the activityName is null or empty</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateElectionInfo([FromBody] CreateElectionInfoModel createElectionInfoModel)
        {
            if (createElectionInfoModel == null)
                return BadRequest(new ApiError(new ApiException.ValueCannotBeNullOrEmptyException(nameof(createElectionInfoModel))));
            ElectionInfo election = _applicationService.GetElectionInfo(createElectionInfoModel.PropertyName).GetAwaiter().GetResult();
            if (election != null) return BadRequest(new ApiError(new ApiException.ElectionInfoExist(createElectionInfoModel.PropertyName)));
            Guid electionId = Guid.NewGuid();
            _applicationService.CreateElectionInfo(createElectionInfoModel.ToElectionInfo(electionId)).GetAwaiter().GetResult();
            return StatusCode(201, electionId);
        }
        
        /// <summary>
        /// Get User
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /{id}/updateUserProfilePhoto
        /// 
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="201">Returns the newly created deeplink</response>
        /// <response code="400">If the url is null or empty</response>
        [HttpPost("/{id:guid}/uploadElectionImage")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<string> UploadElectionImage([FromRoute] Guid id, [FromForm] IFormFile image)
        {
            _imageHelper.ValidateMedia(image, out ApiException.BadRequestException exception);
            if (exception != null) throw exception;
            string directory = "images/" + id + "/";
            const string imageName = "profile-image";
            string fullPath = directory + imageName + Path.GetExtension(image.FileName);
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                DirectoryInfo d = new DirectoryInfo(directory);
                FileInfo[] imageFiles = d.GetFiles();
                FileInfo oldImage = null;
                foreach (var imageFile in imageFiles)
                {
                    if (Path.GetFileNameWithoutExtension(imageFile.Name) == imageName)
                    {
                        oldImage = imageFile;
                        break;
                    }
                }
                oldImage?.Delete();
                using (FileStream filestream = System.IO.File.Create(fullPath))
                {
                    image.CopyTo(filestream);
                    filestream.Flush();
                    return Task.FromResult(fullPath);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.ToString());
            }
        }
        
        /// <summary>
        /// Get User Photo
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /{id}/getUserPhoto
        /// 
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="200">Returns the newly created deeplink</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("/{id:guid}/getElectionImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult GetElectionImage([FromRoute] Guid id)
        {  
            var path = "images/" + id + "/"; 
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] imageFiles;
            try
            {
                imageFiles = d.GetFiles();
            }
            catch (Exception)
            {
                return BadRequest(new ApiError(new ApiException.ElectionInfoNotExist(id.ToString())));
            }
            FileInfo userImage = null;
            foreach (var image in imageFiles)
            {
                if (image.Extension == ".png" || image.Extension == ".jpg" || image.Extension == ".jpeg")
                {
                    if (image.FullName.Contains("profile-image"))
                    {
                        userImage = image;   
                    }
                }
            }
            if (userImage == null) return BadRequest(new ApiError(new ApiException.ElectionImageNotExist()));
            FileStream imageStr = System.IO.File.OpenRead(path + "profile-image" + userImage.Extension);
            return File(imageStr, "image/jpeg");        
        }
        
        /// <summary>
        /// Delete User Photo
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /{id}/deleteProfileImage
        /// 
        /// </remarks>
        /// <returns>Ok</returns>
        /// <response code="200">Returns the newly created deeplink</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("/{id:guid}/deleteElectionImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteElectionImage([FromRoute] Guid id)
        {
            var path = "images/" + id + "/"; 
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] imageFiles = d.GetFiles();
            FileInfo userImage = null;
            foreach (var image in imageFiles)
            {
                if (image.Extension == ".png" || image.Extension == ".jpg" || image.Extension == ".jpeg")
                {
                    userImage = image;
                }
            }

            if (userImage == null)
            {
                return BadRequest(new ApiException.ElectionImageNotExist());
            }
            userImage.Delete();
            return Ok();
        }
    }
}