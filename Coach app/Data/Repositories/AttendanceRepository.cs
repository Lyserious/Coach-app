using Coach_app.Models.Domains.Groups;
using Coach_app.Services.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coach_app.Data.Repositories
{
    // On force explicitement l'interface du dossier Interfaces
    public class AttendanceRepository : Coach_app.Data.Repositories.Interfaces.IAttendanceRepository
    {
        private readonly ICoachDatabaseService _dbService;

        public AttendanceRepository(ICoachDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<SessionAttendance>> GetAttendanceForSessionAsync(int sessionId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return new List<SessionAttendance>();
            return await db.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
        }

        public async Task SaveAttendanceListAsync(List<SessionAttendance> attendanceList)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null || attendanceList == null || !attendanceList.Any()) return;

            foreach (var item in attendanceList)
            {
                if (item.Id > 0) await db.UpdateAsync(item);
                else await db.InsertAsync(item);
            }
        }

        public async Task DeleteAttendanceForSessionAsync(int sessionId)
        {
            var db = await _dbService.GetConnectionAsync();
            if (db == null) return;
            var attendances = await db.Table<SessionAttendance>().Where(a => a.GroupSessionId == sessionId).ToListAsync();
            foreach (var att in attendances) await db.DeleteAsync(att);
        }
    }
}