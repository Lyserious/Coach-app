using IPerformanceRepository = Coach_app.Data.Repositories.Interfaces.IPerformanceRepository;
using IAttendanceRepository = Coach_app.Data.Repositories.Interfaces.IAttendanceRepository;
using IStudentRepository = Coach_app.Data.Repositories.Interfaces.IStudentRepository;

using Coach_app.Models;
using Coach_app.Models.Domains.Groups;
using Coach_app.Models.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coach_app.Services.Training
{
    public interface ISessionComposer
    {
        Task<List<StudentAttendanceItem>> GetAttendanceListAsync(int sessionId, int groupId);
        Task<List<StudentPerformanceItem>> GetPerformanceListAsync(SessionExercise sessionExercise, List<StudentAttendanceItem> currentAttendance);
        string CycleAttendanceStatus(string currentStatus);
    }

    public class SessionComposer : ISessionComposer
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IPerformanceRepository _performanceRepo;

        public SessionComposer(
            IAttendanceRepository attendanceRepo,
            IStudentRepository studentRepo,
            IPerformanceRepository performanceRepo)
        {
            _attendanceRepo = attendanceRepo;
            _studentRepo = studentRepo;
            _performanceRepo = performanceRepo;
        }

        public async Task<List<StudentAttendanceItem>> GetAttendanceListAsync(int sessionId, int groupId)
        {
            var students = await _studentRepo.GetStudentsByGroupIdAsync(groupId);
            var existingAttendance = await _attendanceRepo.GetAttendanceForSessionAsync(sessionId);

            var list = new List<StudentAttendanceItem>();

            foreach (var student in students)
            {
                var record = existingAttendance.FirstOrDefault(a => a.StudentId == student.Id);
                string status = record?.Status ?? "Absent";

                list.Add(new StudentAttendanceItem
                {
                    StudentId = student.Id,
                    DisplayName = student.DisplayName,
                    PhotoPath = student.ProfilePhotoPath,
                    AttendanceId = record?.Id ?? 0,
                    Status = status,
                    Note = record?.Note
                });
            }
            return list;
        }

        public async Task<List<StudentPerformanceItem>> GetPerformanceListAsync(SessionExercise sessionExercise, List<StudentAttendanceItem> currentAttendance)
        {
            if (sessionExercise == null) return new List<StudentPerformanceItem>();

            var existingPerfs = await _performanceRepo.GetPerformancesBySessionExerciseAsync(sessionExercise.Id);

            var presentStudents = currentAttendance
                .Where(a => !string.Equals(a.Status, "Absent", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var scoringType = sessionExercise.Exercise.ScoringType;
            var list = new List<StudentPerformanceItem>();

            foreach (var s in presentStudents)
            {
                var p = existingPerfs.FirstOrDefault(x => x.StudentId == s.StudentId && x.SetNumber == 1);

                var item = new StudentPerformanceItem
                {
                    StudentId = s.StudentId,
                    Name = s.DisplayName,
                    PhotoPath = s.PhotoPath,
                    PerfId = p?.Id ?? 0,
                    IsNumeric = scoringType == PerformanceType.Numeric,
                    IsCompletion = scoringType == PerformanceType.Completion,
                    IsLevel = scoringType == PerformanceType.Level
                };

                if (item.IsCompletion) item.IsCompleted = p?.Value == "true";
                else item.ValueDisplay = p?.Value ?? "";

                list.Add(item);
            }
            return list;
        }

        public string CycleAttendanceStatus(string currentStatus)
        {
            return currentStatus switch
            {
                "Absent" => "Présent",
                "Présent" => "Retard",
                "Retard" => "Absent",
                _ => "Présent"
            };
        }
    }
}