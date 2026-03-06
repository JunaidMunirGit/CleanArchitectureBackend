using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPosOrders : Migration
{
    private static readonly string[] OrdersCreatedAtStatus = ["created_at", "status"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "orders",
            schema: "dbo",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                order_number = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                status = table.Column<int>(type: "int", nullable: false),
                sub_total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                tax_amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                discount_amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                payment_method = table.Column<int>(type: "int", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "product_inventories",
            schema: "dbo",
            columns: table => new
            {
                product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                quantity_on_hand = table.Column<int>(type: "int", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_product_inventories", x => x.product_id);
            });

        migrationBuilder.CreateTable(
            name: "order_lines",
            schema: "dbo",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                quantity = table.Column<int>(type: "int", nullable: false),
                unit_price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                line_total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_lines", x => x.id);
                table.ForeignKey(
                    name: "fk_order_lines_orders_order_id",
                    column: x => x.order_id,
                    principalSchema: "dbo",
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "order_payments",
            schema: "dbo",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                method = table.Column<int>(type: "int", nullable: false),
                amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_payments", x => x.id);
                table.ForeignKey(
                    name: "fk_order_payments_orders_order_id",
                    column: x => x.order_id,
                    principalSchema: "dbo",
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_order_lines_order_id",
            schema: "dbo",
            table: "order_lines",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_order_payments_order_id",
            schema: "dbo",
            table: "order_payments",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_created_at_status",
            schema: "dbo",
            table: "orders",
            columns: OrdersCreatedAtStatus);

        migrationBuilder.CreateIndex(
            name: "ix_orders_order_number",
            schema: "dbo",
            table: "orders",
            column: "order_number",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "order_lines",
            schema: "dbo");

        migrationBuilder.DropTable(
            name: "order_payments",
            schema: "dbo");

        migrationBuilder.DropTable(
            name: "product_inventories",
            schema: "dbo");

        migrationBuilder.DropTable(
            name: "orders",
            schema: "dbo");
    }
}
