using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniRAG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "DocumentMetadata",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DocumentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
            //        UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DocumentMetadata", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AzureObjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProfileCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentMetadata");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
