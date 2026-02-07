using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Folders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FolderItemType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StorageId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderItems_FolderItems_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "FolderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderItems_ParentFolderId",
                table: "FolderItems",
                column: "ParentFolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderItems");
        }
    }
}
