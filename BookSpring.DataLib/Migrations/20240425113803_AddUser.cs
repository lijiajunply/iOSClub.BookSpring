using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSpring.DataLib.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "LendTo",
                table: "Books",
                newName: "LendToId");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnDate",
                table: "Books",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "LendDate",
                table: "Books",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Books",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(10)", nullable: false),
                    Name = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_CreatedById",
                table: "Books",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LendToId",
                table: "Books",
                column: "LendToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_CreatedById",
                table: "Books",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_LendToId",
                table: "Books",
                column: "LendToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_CreatedById",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_LendToId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Books_CreatedById",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_LendToId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "LendToId",
                table: "Books",
                newName: "LendTo");

            migrationBuilder.AlterColumn<string>(
                name: "ReturnDate",
                table: "Books",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LendDate",
                table: "Books",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Books",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "");
        }
    }
}
