using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KuroFinance.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
