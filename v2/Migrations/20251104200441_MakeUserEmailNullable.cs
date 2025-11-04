using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    public partial class MakeUserEmailNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename old table
            migrationBuilder.Sql("ALTER TABLE Users RENAME TO Users_old;");

            // Create new Users table with Email nullable
            migrationBuilder.Sql(@"
CREATE TABLE Users (
    ID TEXT NOT NULL PRIMARY KEY,
    OldID TEXT NOT NULL,
    IdentityUserId TEXT NOT NULL,
    Username TEXT NOT NULL,
    Name TEXT NOT NULL,
    Email TEXT,
    PhoneNumber TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    BirthDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    CONSTRAINT FK_Users_IdentityUser FOREIGN KEY (IdentityUserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);");

            // Copy data from old table
            migrationBuilder.Sql(@"
INSERT INTO Users (ID, OldID, IdentityUserId, Username, Name, Email, PhoneNumber, Role, CreatedAt, BirthDate, IsActive)
SELECT ID, OldID, IdentityUserId, Username, Name, Email, PhoneNumber, Role, CreatedAt, BirthDate, IsActive
FROM Users_old;");

            // Drop old table
            migrationBuilder.Sql("DROP TABLE Users_old;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to NOT NULL Email
            migrationBuilder.Sql("ALTER TABLE Users RENAME TO Users_new;");

            migrationBuilder.Sql(@"
CREATE TABLE Users (
    ID TEXT NOT NULL PRIMARY KEY,
    OldID TEXT NOT NULL,
    IdentityUserId TEXT NOT NULL,
    Username TEXT NOT NULL,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    Role TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    BirthDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    CONSTRAINT FK_Users_IdentityUser FOREIGN KEY (IdentityUserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);");

            migrationBuilder.Sql(@"
INSERT INTO Users (ID, OldID, IdentityUserId, Username, Name, Email, PhoneNumber, Role, CreatedAt, BirthDate, IsActive)
SELECT ID, OldID, IdentityUserId, Username, Name, COALESCE(Email, ''), PhoneNumber, Role, CreatedAt, BirthDate, IsActive
FROM Users_new;");

            migrationBuilder.Sql("DROP TABLE Users_new;");
        }
    }
}