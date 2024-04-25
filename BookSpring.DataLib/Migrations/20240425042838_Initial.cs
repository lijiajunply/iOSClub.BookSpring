using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSpring.DataLib.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "varchar(64)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(10)", nullable: false),
                    LendTo = table.Column<string>(type: "varchar(10)", nullable: false),
                    LendDate = table.Column<string>(type: "varchar(10)", nullable: false),
                    ReturnDate = table.Column<string>(type: "varchar(10)", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(64)", nullable: false),
                    Description = table.Column<string>(type: "varchar(10)", nullable: false),
                    Category = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsEBook = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
