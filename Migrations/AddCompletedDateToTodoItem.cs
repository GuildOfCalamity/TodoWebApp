using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedDateToTodoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "TodoItems",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "TodoItems");
        }
    }
}
