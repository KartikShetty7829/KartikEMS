namespace EmployeeService.domains
{
    using global::EmployeeServiceDepartmentService.domains;
    using MediatR;

    namespace EmployeeServiceDepartmentService.domains
    {
        public class EmployeeProfileByIdRequestDto : IRequest<EmployeeProfileResponseDto>
        {
            public Guid EmployeeId { get; set; }
        }
    }

}
