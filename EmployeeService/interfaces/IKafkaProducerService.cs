using System.Threading.Tasks;

namespace EmployeeServiceDepartmentService.interfaces
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync(string topic, string message);
    }
}
