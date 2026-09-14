// Administrator accounts can't self-register (see Stakeholders.Api's
// AuthController) and must be inserted directly into the database, per spec.
//
// Usage (with the docker-compose stack running):
//   docker compose exec -T mongo mongosh stakeholders_db < scripts/seed-admin.js
//
// Default login: username "admin", password "Admin123!" — change the password
// hash below (see comment) if you need different credentials.

db.users.insertOne({
  Username: "admin",
  Email: "admin@soa-turista.local",
  // bcrypt hash of "Admin123!". To generate a hash for a different password,
  // run: dotnet run — with a throwaway console project referencing
  // BCrypt.Net-Next — and call BCrypt.Net.BCrypt.HashPassword("<password>").
  PasswordHash: "$2a$11$9GiFrES3jc6p.hJmd4VzZOnAzVegR2eOM.OOQyPnyR2..2sFK0N5C",
  // MongoDB.Driver stores this enum as its ordinal, not its name:
  // Tourist = 0, Guide = 1, Administrator = 2.
  Role: 2,
  Profile: {
    FirstName: "Admin",
    LastName: "Admin",
    ProfileImagePath: null,
    Biography: null,
    Motto: null
  }
});
