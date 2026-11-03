using EmployeeServiceDepartmentService.domains;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace EmployeeServiceDepartmentService.Domains
{
    public class ProfilePictureUploadRequest : IRequest<ProfilePictureUploadResponse>
    {
        public int EmployeeNumber { get; set; }
        public IFormFile File { get; set; }
    }
}
