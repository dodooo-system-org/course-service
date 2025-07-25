using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace course_service.Migrations
{
    /// <inheritdoc />
    public partial class Migration_Jul_25_21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseEntityCourseId",
                table: "Modules",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseEntityCourseId",
                table: "Modules",
                column: "CourseEntityCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Courses_CourseEntityCourseId",
                table: "Modules",
                column: "CourseEntityCourseId",
                principalTable: "Courses",
                principalColumn: "course_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Courses_CourseEntityCourseId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_CourseEntityCourseId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "CourseEntityCourseId",
                table: "Modules");
        }
    }
}
