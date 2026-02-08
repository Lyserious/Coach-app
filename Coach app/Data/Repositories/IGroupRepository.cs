using Coach_app.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetActiveGroupsAsync();
        Task<Group> GetGroupByIdAsync(int id);
        Task<int> SaveGroupAsync(Group group);
        Task<int> DeleteGroupAsync(int id);

        Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId);
        Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList);
        Task<List<GroupSession>> GetSessionsByGroupIdAsync(int groupId);
        Task<List<GroupSession>> GetSessionsByDateAsync(DateTime date);
        Task GenerateSessionsForGroupAsync(Group group);
        Task<GroupSession> GetSessionByIdAsync(int id);
    }
}