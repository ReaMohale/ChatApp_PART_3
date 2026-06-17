using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp_Part3
{
    public class SecurityTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsComplete { get; set; }
    }

    public class TaskStore
    {
        private List<SecurityTask> tasks = new List<SecurityTask>();
        private string connectionString;

        public TaskStore()
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SecureShieldDB"]?.ConnectionString
                ?? @"Server=localhost;Database=SecureShieldDB;Integrated Security=True;";
        }

        public async Task LoadTasks()
        {
            try
            {
                tasks.Clear();
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT Id, Title, Description, CreatedAt, ReminderDate, IsComplete FROM Tasks", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tasks.Add(new SecurityTask
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        ReminderDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        IsComplete = reader.GetBoolean(5)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTasks error: {ex.Message}");
                // Fallback to in-memory only
                if (!tasks.Any())
                {
                    tasks.Add(new SecurityTask
                    {
                        Id = 1,
                        Title = "Enable two-factor authentication",
                        Description = "Enable two-factor authentication on all accounts",
                        CreatedAt = DateTime.Now,
                        IsComplete = false
                    });
                    tasks.Add(new SecurityTask
                    {
                        Id = 2,
                        Title = "Review privacy settings",
                        Description = "Review account privacy settings and adjust as needed",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        IsComplete = false
                    });
                }
            }
        }

        public async Task AddTask(SecurityTask task)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "INSERT INTO Tasks (Title, Description, CreatedAt, ReminderDate, IsComplete) VALUES (@Title, @Description, @CreatedAt, @ReminderDate, @IsComplete); SELECT SCOPE_IDENTITY();",
                    conn);
                cmd.Parameters.AddWithValue("@Title", task.Title);
                cmd.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", task.CreatedAt);
                cmd.Parameters.AddWithValue("@ReminderDate", (object?)task.ReminderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsComplete", task.IsComplete);
                task.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                tasks.Add(task);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTask error: {ex.Message}");
                // Fallback: assign in-memory ID
                task.Id = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
                tasks.Add(task);
            }
        }

        public async Task UpdateTask(SecurityTask task)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "UPDATE Tasks SET Title=@Title, Description=@Description, ReminderDate=@ReminderDate, IsComplete=@IsComplete WHERE Id=@Id",
                    conn);
                cmd.Parameters.AddWithValue("@Id", task.Id);
                cmd.Parameters.AddWithValue("@Title", task.Title);
                cmd.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReminderDate", (object?)task.ReminderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsComplete", task.IsComplete);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTask error: {ex.Message}");
            }

            var existing = tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existing != null)
            {
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.ReminderDate = task.ReminderDate;
                existing.IsComplete = task.IsComplete;
            }
        }

        public async Task DeleteTask(int taskId)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM Tasks WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", taskId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteTask error: {ex.Message}");
            }

            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null) tasks.Remove(task);
        }

        public async Task MarkTaskComplete(int taskId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsComplete = true;
                await UpdateTask(task);
            }
        }

        public SecurityTask? GetTask(int taskId) => tasks.FirstOrDefault(t => t.Id == taskId);

        public List<SecurityTask> GetAllTasks() => tasks.ToList();
    }
}