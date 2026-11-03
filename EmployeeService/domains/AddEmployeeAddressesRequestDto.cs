using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    // Request DTO (same class name, now supports multiple employees)
    public class AddEmployeeAddressesRequestDto : IRequest<AddEmployeeAddressesResponseDto>
    {
        public List<EmployeeAddressesDto> Employees { get; set; } 
    }

    // Employee + Addresses grouping
    public class EmployeeAddressesDto
    {
        public int EmployeeNumber { get; set; }   // Employee identifier from UI
        public List<AddressDto> Addresses { get; set; }  // Multiple addresses
    }

    // Address details
    public class AddressDto
    {
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string AddressType { get; set; }
    }
}
