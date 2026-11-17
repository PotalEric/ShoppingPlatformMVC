using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingPlatformMVC.Migrations;

/// <inheritdoc />
public partial class _20261117062930_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Carts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DemoCarts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Product",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Stock = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Product", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "CartItems",
            columns: table => new
            {
                CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DemoCartItems", x => new { x.CartId, x.ProductId });
                table.ForeignKey(
                    name: "FK_DemoCartItems_DemoCarts",
                    column: x => x.CartId,
                    principalTable: "Carts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductPhotos",
            columns: table => new
            {
                ProductId = table.Column<int>(type: "int", nullable: false),
                Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPhotos", x => x.ProductId);
                table.ForeignKey(
                    name: "FK_ProductPhotos_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CartItems");

        migrationBuilder.DropTable(
            name: "ProductPhotos");

        migrationBuilder.DropTable(
            name: "Carts");

        migrationBuilder.DropTable(
            name: "Product");
    }
}
