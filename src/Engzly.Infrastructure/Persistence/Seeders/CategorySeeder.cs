using Engzly.Domain.Entities.Gigs;
using Engzly.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Engzly.Infrastructure.Persistence.Seeders
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(EngzlyDbContext context, CancellationToken ct = default)
        {
            var seeds = new[]
            {
                new Category
                {
                    Id = "events",
                    Name = "Events",
                    Description = "Event planning and support. Weddings, birthdays, parties, conferences. Setup, decoration, catering help, photography, hosting, cleanup."
                },
                new Category
                {
                    Id = "delivery",
                    Name = "Delivery",
                    Description = "Picking up and delivering items between locations. Packages, documents, groceries, food, furniture, online orders, gifts."
                },
                new Category
                {
                    Id = "government-papers",
                    Name = "Government Papers",
                    Description = "Paperwork and official documents. ID renewal, licenses, permits, notary, ministry visits, embassy appointments, legal forms, standing in line at government offices."
                },
                new Category
                {
                    Id = "fix-things",
                    Name = "Fix Things",
                    Description = "Repairs and maintenance. Appliances, plumbing, electrical, furniture assembly, broken devices, air conditioning, leaks, painting, small construction."
                },
                new Category
                {
                    Id = "cleaning",
                    Name = "Cleaning",
                    Description = "House, office, and post-event cleaning. Deep cleaning, laundry, ironing, dishwashing, window cleaning, carpet cleaning."
                },
                new Category
                {
                    Id = "moving-transport",
                    Name = "Moving & Transport",
                    Description = "Moving homes or offices. Carrying heavy items, loading and unloading, furniture relocation, short-distance transport of goods."
                },
                new Category
                {
                    Id = "shopping-errands",
                    Name = "Shopping & Errands",
                    Description = "Buying items from stores, grocery runs, bill payments, pharmacy pickups, picking up orders, running errands around the city."
                },
                new Category
                {
                    Id = "tutoring-lessons",
                    Name = "Tutoring & Lessons",
                    Description = "Teaching and academic help. School subjects, university courses, languages, exam prep, homework help, music, tutoring kids."
                },
                new Category
                {
                    Id = "tech-help",
                    Name = "Tech Help",
                    Description = "Computer, phone, and software support. Setting up devices, installing software, fixing bugs, network and WiFi issues, printer setup, account recovery."
                },
                new Category
                {
                    Id = "pet-home-care",
                    Name = "Pet & Home Care",
                    Description = "Pet sitting, dog walking, feeding animals, plant watering, checking on the house while away, babysitting help."
                },
            };

            var existingIds = await context.Categories
                .Select(c => c.Id)
                .ToListAsync(ct);

            var missing = seeds.Where(s => !existingIds.Contains(s.Id)).ToList();
            if (missing.Count == 0)
                return;

            await context.Categories.AddRangeAsync(missing, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}
