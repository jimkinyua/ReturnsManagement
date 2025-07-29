using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class ChangeWorkFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "CanBeSeen",
                table: "WorkflowInstances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PeriodId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PeriodId",
                table: "ApprovalActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnSubmissionId",
                table: "ApprovalActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaccoId",
                table: "ApprovalActions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "CanBeSeen",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "PeriodId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "PeriodId",
                table: "ApprovalActions");

            migrationBuilder.DropColumn(
                name: "ReturnSubmissionId",
                table: "ApprovalActions");

            migrationBuilder.DropColumn(
                name: "SaccoId",
                table: "ApprovalActions");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
