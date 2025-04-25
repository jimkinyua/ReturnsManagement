using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Returns.Migrations
{
    /// <inheritdoc />
    public partial class MissingWorflowFieds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "WorkflowInstances");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStepId",
                table: "WorkflowInstances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeamId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CurrentStepId",
                table: "WorkflowInstances",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances",
                column: "ReturnId",
                principalTable: "Returns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkFlowSteps_CurrentStepId",
                table: "WorkflowInstances",
                column: "CurrentStepId",
                principalTable: "WorkFlowSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Returns_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkFlowSteps_CurrentStepId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_CurrentStepId",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_ReturnId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WorkflowInstances");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStepId",
                table: "WorkflowInstances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "WorkflowInstances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
