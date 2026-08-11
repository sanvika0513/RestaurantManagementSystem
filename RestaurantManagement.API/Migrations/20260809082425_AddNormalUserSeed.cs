using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bacdc2fd-3967-4727-89c7-38cf2e911bfc");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2d9acf7f-9864-4c53-b851-8e508bee487f", "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e", "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e", "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2d9acf7f-9864-4c53-b851-8e508bee487f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5d290043-3ada-40df-b04d-8841285dc377", null, "SuperAdmin", "SUPERADMIN" },
                    { "8a5b930f-166c-43cd-a09d-70ae8c2621ea", null, "RestaurantAdmin", "RESTAURANTADMIN" },
                    { "d2a48baa-9c68-4908-8d32-528d1bb93599", null, "User", "USER" }
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ed7e303b-8be3-4a02-96d7-1953ae5c1626", "AQAAAAIAAYagAAAAEBBHNKjzp89S4PNHX4GG29OgNKxzkF59wNmJ2X2a6k5E0qK7R54d8530yzhARMms/Q==", "7bbbbbf2-ecf0-473e-bc75-a9df4d1d6134" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6fabfa6b-e921-419c-9923-0539749c7602", "AQAAAAIAAYagAAAAEG/4774dUD5QnE+/EC3Il0SMbC9IRSv37jD6N88AamOZA5JQOu7WGpOHjcoiKrVKpw==", "8eaa2b8c-8c8f-4c78-8524-3bd129aa57d5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "11a5dadc-4c6b-43ae-8a59-115617f894f0", "AQAAAAIAAYagAAAAEL98xwxAGAsju5l6IQJ/kUPkimak5nTQCcwg+gPqjE4YK6EvrUyC3peVckKiV/RCbA==", "feb3edfb-0b35-4b3a-a88d-8ae889cdea53" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RestaurantId", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "a1b2c3d4-e5f6-4711-8c9d-0a1b2c3d4e5f", 0, "a7a8c8f8-0e06-44ae-86e9-967783c69d88", "user@restaurant.local", true, false, null, "USER@RESTAURANT.LOCAL", "USER1", "AQAAAAIAAYagAAAAEHEXa43W3HxYRdnIWA4TH8fueOxNaBy/MEevTaFZX4d0c5r+LNQfJXh6R8cgKxxKaQ==", null, false, null, "a5ffd625-b59d-4f1a-b424-1eb73dd33fe4", false, "User1" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "5d290043-3ada-40df-b04d-8841285dc377", "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a" },
                    { "8a5b930f-166c-43cd-a09d-70ae8c2621ea", "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3" },
                    { "d2a48baa-9c68-4908-8d32-528d1bb93599", "a1b2c3d4-e5f6-4711-8c9d-0a1b2c3d4e5f" },
                    { "8a5b930f-166c-43cd-a09d-70ae8c2621ea", "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_MenuItemId",
                table: "CartItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId_MenuItemId",
                table: "CartItems",
                columns: new[] { "UserId", "MenuItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "5d290043-3ada-40df-b04d-8841285dc377", "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8a5b930f-166c-43cd-a09d-70ae8c2621ea", "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "d2a48baa-9c68-4908-8d32-528d1bb93599", "a1b2c3d4-e5f6-4711-8c9d-0a1b2c3d4e5f" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8a5b930f-166c-43cd-a09d-70ae8c2621ea", "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d290043-3ada-40df-b04d-8841285dc377");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8a5b930f-166c-43cd-a09d-70ae8c2621ea");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d2a48baa-9c68-4908-8d32-528d1bb93599");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4711-8c9d-0a1b2c3d4e5f");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "MenuItems");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2d9acf7f-9864-4c53-b851-8e508bee487f", null, "SuperAdmin", "SUPERADMIN" },
                    { "bacdc2fd-3967-4727-89c7-38cf2e911bfc", null, "User", "USER" },
                    { "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e", null, "RestaurantAdmin", "RESTAURANTADMIN" }
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e9544e61-4681-4771-a10b-9571f5ca1981", "AQAAAAIAAYagAAAAENJvqn1sgFNt/ghh2SliwbaVfLpASznElTYlVGJMCvurhiSg23tgKM/IrvKhMd7y/Q==", "a3d48551-7df3-4983-9c82-3a66a4ab0091" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "53d7f4be-ab9a-4aa5-add6-55e8bc559c5a", "AQAAAAIAAYagAAAAEK4F7zj1CzA6HNZiCahzjDNt7MfmN1xwTcxEiPWx14hInjsTgdWLPMYW6ZnkELH9tw==", "c624f63d-c378-47d9-87e5-569aad1a7eef" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "40ed3fa1-db9a-4ce1-9cda-c9252c24cf05", "AQAAAAIAAYagAAAAEM+Em0XKWoQtQapbyQT6QJDFDcroZ/yARgCLit35dtD+D+z0fLYpaVdmCkgSKKV51w==", "ac230530-3315-4984-b5d1-a482d9a21109" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "2d9acf7f-9864-4c53-b851-8e508bee487f", "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a" },
                    { "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e", "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3" },
                    { "d78cc3fd-b6a9-4b57-9bd0-ba512ffd222e", "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9" }
                });
        }
    }
}
