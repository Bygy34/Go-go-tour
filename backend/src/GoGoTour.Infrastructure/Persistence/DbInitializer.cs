namespace GoGoTour.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(GoGoTourDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
}
