public static class EmailTemplateService
{
    public static string GetWelcomeEmployeeTemplate(
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime hireDate)
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Templates",
            "WelcomeEmployee.html");

        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "Templates", "WelcomeEmployee.html");
        }

        string html = File.Exists(path)
            ? File.ReadAllText(path)
            : "<h2>Welcome {{FirstName}} {{LastName}}</h2><p>Email: {{Email}}</p><p>Phone: {{Phone}}</p><p>Hire Date: {{HireDate}}</p>";

        html = html.Replace("{{FirstName}}", firstName);
        html = html.Replace("{{LastName}}", lastName);
        html = html.Replace("{{Email}}", email);
        html = html.Replace("{{Phone}}", phone);
        html = html.Replace("{{HireDate}}",
            hireDate.ToString("dd-MMM-yyyy"));

        return html;
    }
}