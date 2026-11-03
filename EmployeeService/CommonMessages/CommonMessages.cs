namespace EmployeeServiceDepartmentService.Common
{
    public static class CommonMessages
    {
        // Success messages
        public const string ProfileFetchSuccess = "Profile fetched successfully.";

        // Error messages
        public const string EmployeeNotFound = "Employee with the given EmployeeNumber was not found.";
        public const string ProfileFetchError = "An error occurred while fetching employee profile.";
        public const string EmployeeUpdate = "Employee Details Updated Successfully.";
        public const string Employeedelete = "the given employeenumber data deleted in the database";
         
        public const string EmployeeMarkedInactive = "the given employeenumber data deleted ";
        public const string EmployeeMarkedActive = "the given employeenumber marked has active";
    }
}

