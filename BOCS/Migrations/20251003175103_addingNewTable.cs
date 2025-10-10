using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOCS.Migrations
{
    /// <inheritdoc />
    public partial class addingNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseLessonAttachment_CourseLessons_CourseLessonId",
                table: "CourseLessonAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseLessonAttachment",
                table: "CourseLessonAttachment");

            migrationBuilder.RenameTable(
                name: "CourseLessonAttachment",
                newName: "LessonAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_CourseLessonAttachment_CourseLessonId",
                table: "LessonAttachment",
                newName: "IX_LessonAttachment_CourseLessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonAttachment",
                table: "LessonAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonAttachment_CourseLessons_CourseLessonId",
                table: "LessonAttachment",
                column: "CourseLessonId",
                principalTable: "CourseLessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonAttachment_CourseLessons_CourseLessonId",
                table: "LessonAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonAttachment",
                table: "LessonAttachment");

            migrationBuilder.RenameTable(
                name: "LessonAttachment",
                newName: "CourseLessonAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_LessonAttachment_CourseLessonId",
                table: "CourseLessonAttachment",
                newName: "IX_CourseLessonAttachment_CourseLessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseLessonAttachment",
                table: "CourseLessonAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLessonAttachment_CourseLessons_CourseLessonId",
                table: "CourseLessonAttachment",
                column: "CourseLessonId",
                principalTable: "CourseLessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
