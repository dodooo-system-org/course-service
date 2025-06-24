using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_service.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Modules",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Modules",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Modules",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Lessons",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Lessons",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Lessons",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "LessonContents",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "LessonContents",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "LessonContents",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Enrollments",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Enrollments",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Enrollments",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Courses",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Courses",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Courses",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Categories",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "Categories",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Categories",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "LessonContents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "LessonContents");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "LessonContents");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Categories");
        }
    }
}
