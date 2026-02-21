using KuroFinance.Data.Entities;
using KuroFinance.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KuroFinance.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByGoogleIdAsync(string googleId) =>
        db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    public async Task AddAsync(User user) => await db.Users.AddAsync(user);

    public Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
