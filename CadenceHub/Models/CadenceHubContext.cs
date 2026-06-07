using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CadenceHub.Models;

public partial class CadenceHubContext : DbContext
{
    public CadenceHubContext()
    {
    }

    public CadenceHubContext(DbContextOptions<CadenceHubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    public virtual DbSet<AttendanceStatus> AttendanceStatuses { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<DutySchedule> DutySchedules { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VAttendanceDailyDetail> VAttendanceDailyDetails { get; set; }

    public virtual DbSet<VAttendanceDailySummary> VAttendanceDailySummaries { get; set; }

    public virtual DbSet<VAttendanceMonthlyStaffSummary> VAttendanceMonthlyStaffSummaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=ADMIN-PC\\MSSQLSERVER1;Initial Catalog=CadenceHub;User ID=sa;Password=Dung@123;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Key);

            entity.ToTable("app_settings");

            entity.Property(e => e.Key)
                .HasMaxLength(100)
                .HasColumnName("key");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Value)
                .HasMaxLength(1000)
                .HasColumnName("value");
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("attendance_records");

            entity.HasIndex(e => new { e.AttendanceDate, e.StaffId }, "UQ_attendance_records_date_staff").IsUnique();

            entity.HasIndex(e => e.AttendanceDate, "idx_attendance_records_date");

            entity.HasIndex(e => e.StaffId, "idx_attendance_records_staff_id");

            entity.HasIndex(e => e.StatusId, "idx_attendance_records_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DutyScheduleId).HasColumnName("duty_schedule_id");
            entity.Property(e => e.EnteredByUserId).HasColumnName("entered_by_user_id");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedByUserId).HasColumnName("updated_by_user_id");

            entity.HasOne(d => d.DutySchedule).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.DutyScheduleId)
                .HasConstraintName("FK_attendance_records_duty_schedule");

            entity.HasOne(d => d.EnteredByUser).WithMany(p => p.AttendanceRecordEnteredByUsers)
                .HasForeignKey(d => d.EnteredByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_attendance_records_entered_by");

            entity.HasOne(d => d.Staff).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_attendance_records_staff");

            entity.HasOne(d => d.Status).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_attendance_records_status");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.AttendanceRecordUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_attendance_records_updated_by");
        });

        modelBuilder.Entity<AttendanceStatus>(entity =>
        {
            entity.ToTable("attendance_statuses");

            entity.HasIndex(e => e.Code, "UQ_attendance_statuses_code").IsUnique();

            entity.HasIndex(e => e.Name, "UQ_attendance_statuses_name").IsUnique();

            entity.HasIndex(e => e.SortOrder, "UQ_attendance_statuses_sort_order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.IsAbsentGroup).HasColumnName("is_absent_group");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPresentGroup).HasColumnName("is_present_group");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasIndex(e => e.CreatedAt, "idx_audit_logs_created_at");

            entity.HasIndex(e => new { e.EntityName, e.EntityId }, "idx_audit_logs_entity");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionCode)
                .HasMaxLength(100)
                .HasColumnName("action_code");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .HasColumnName("entity_name");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.OldValue).HasColumnName("old_value");

            entity.HasOne(d => d.ActorUser).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("FK_audit_logs_actor");
        });

        modelBuilder.Entity<DutySchedule>(entity =>
        {
            entity.ToTable("duty_schedules");

            entity.HasIndex(e => new { e.DutyDate, e.ShiftCode, e.StaffId }, "UQ_duty_schedules_date_shift_staff").IsUnique();

            entity.HasIndex(e => e.DutyDate, "idx_duty_schedules_date");

            entity.HasIndex(e => e.StaffId, "idx_duty_schedules_staff_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedByUserId).HasColumnName("assigned_by_user_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DutyDate).HasColumnName("duty_date");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.ShiftCode)
                .HasMaxLength(20)
                .HasDefaultValue("FULL_DAY")
                .HasColumnName("shift_code");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AssignedByUser).WithMany(p => p.DutySchedules)
                .HasForeignKey(d => d.AssignedByUserId)
                .HasConstraintName("FK_duty_schedules_assigned_by");

            entity.HasOne(d => d.Staff).WithMany(p => p.DutySchedules)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_duty_schedules_staff");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasIndex(e => e.Code, "UQ_roles_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("staff");

            entity.HasIndex(e => e.StaffCode, "UQ_staff_staff_code").IsUnique();

            entity.HasIndex(e => e.FullName, "idx_staff_full_name");

            entity.HasIndex(e => e.Unit, "idx_staff_unit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .HasColumnName("position_code");
            entity.Property(e => e.PositionName)
                .HasMaxLength(255)
                .HasColumnName("position_name");
            entity.Property(e => e.StaffCode)
                .HasMaxLength(50)
                .HasColumnName("staff_code");
            entity.Property(e => e.Unit)
                .HasMaxLength(100)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_accounts");

            entity.HasIndex(e => e.Username, "UQ_user_accounts_username").IsUnique();

            entity.HasIndex(e => e.StaffId, "idx_user_accounts_staff_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(255)
                .HasColumnName("display_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt)
                .HasPrecision(0)
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .HasColumnName("password_hash");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Staff).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_user_accounts_staff");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.ToTable("user_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_user_roles_roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_user_roles_user_accounts");
        });

        modelBuilder.Entity<VAttendanceDailyDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_attendance_daily_detail");

            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasColumnName("created_at");
            entity.Property(e => e.EnteredBy)
                .HasMaxLength(100)
                .HasColumnName("entered_by");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .HasColumnName("position_code");
            entity.Property(e => e.PositionName)
                .HasMaxLength(255)
                .HasColumnName("position_name");
            entity.Property(e => e.StaffCode)
                .HasMaxLength(50)
                .HasColumnName("staff_code");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(100)
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(255)
                .HasColumnName("status_name");
            entity.Property(e => e.Unit)
                .HasMaxLength(100)
                .HasColumnName("unit");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<VAttendanceDailySummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_attendance_daily_summary");

            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(100)
                .HasColumnName("status_code");
            entity.Property(e => e.StatusName)
                .HasMaxLength(255)
                .HasColumnName("status_name");
            entity.Property(e => e.TotalCount).HasColumnName("total_count");
        });

        modelBuilder.Entity<VAttendanceMonthlyStaffSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_attendance_monthly_staff_summary");

            entity.Property(e => e.AbsentDays).HasColumnName("absent_days");
            entity.Property(e => e.AttendanceRatePercent)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attendance_rate_percent");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.PresentDays).HasColumnName("present_days");
            entity.Property(e => e.RecordedDays).HasColumnName("recorded_days");
            entity.Property(e => e.ReportMonth)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("report_month");
            entity.Property(e => e.StaffCode)
                .HasMaxLength(50)
                .HasColumnName("staff_code");
            entity.Property(e => e.Unit)
                .HasMaxLength(100)
                .HasColumnName("unit");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
