using MediatR;

namespace EmployeeServiceDepartmentService.domains
{
    public class GenerateLeaveSummaryReportRequestDto: IRequest<GenerateLeaveSummaryReportResponseDto>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int? Day { get; set; } // optional filter
        public  required string Format { get; set; } // "PDF" or "Excel"
    }

}
