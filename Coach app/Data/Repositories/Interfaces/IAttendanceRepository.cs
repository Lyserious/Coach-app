using Coach_app.Models.Domains.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId);
        Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList);
        Task DeleteAttendanceForSessionAsync(int sessionId);
    }
}