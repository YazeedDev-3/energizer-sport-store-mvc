using project_web2.Models;

namespace project_web2.Data
{
    public static class DbInitializer
    {
        public static void Initialize(project_web2Context context)
        {
            if (context.Users.Any())
                return;

            var admin = new Users
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "admin"
            };
            var customer = new Users
            {
                Username = "customer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                Role = "customer"
            };
            context.Users.AddRange(admin, customer);
            context.SaveChanges();

            context.CustomerProfiles.Add(new CustomerProfiles
            {
                UserId = customer.Id,
                FullName = "Customer Demo",
                Email = "customer@example.com",
                Gender = "male",
                City = "Riyadh"
            });
            context.SaveChanges();

            context.Products.AddRange(
                new Products
                {
                    Name = "Energizer Whey Protein Chocolate",
                    Description = "High-quality whey protein in rich chocolate flavor to support muscle growth and recovery.",
                    Price = 150, Discount = 0, CategoryId = 1, Stock = 100,
                    ImageFile = "Chocolate_Protein.png", CreatedAt = DateTime.Now
                },
                new Products
                {
                    Name = "Energizer Whey Protein Vanilla",
                    Description = "Premium whey protein with a smooth vanilla taste for post-workout nutrition.",
                    Price = 150, Discount = 0, CategoryId = 1, Stock = 80,
                    ImageFile = "Vanilla_Protein.png", CreatedAt = DateTime.Now
                },
                new Products
                {
                    Name = "Energizer Creatine Monohydrate",
                    Description = "Pure creatine monohydrate to enhance strength, power, and athletic performance.",
                    Price = 35, Discount = 0, CategoryId = 2, Stock = 150,
                    ImageFile = "Creatine.png", CreatedAt = DateTime.Now
                },
                new Products
                {
                    Name = "Energizer Pre-Workout",
                    Description = "Explosive pre-workout formula for maximum energy, focus, and endurance.",
                    Price = 50, Discount = 0, CategoryId = 3, Stock = 120,
                    ImageFile = "Pre-Workout.png", CreatedAt = DateTime.Now
                },
                new Products
                {
                    Name = "Energizer Sport Vitamins",
                    Description = "Complete multivitamin and mineral blend formulated for active athletes.",
                    Price = 40, Discount = 0, CategoryId = 4, Stock = 200,
                    ImageFile = "Vitamins.png", CreatedAt = DateTime.Now
                },
                new Products
                {
                    Name = "Energizer Sport Snack Bar",
                    Description = "High-protein snack bar for on-the-go nutrition between meals and training sessions.",
                    Price = 20, Discount = 0, CategoryId = 5, Stock = 300,
                    ImageFile = "Snack.png", CreatedAt = DateTime.Now
                }
            );
            context.SaveChanges();
        }
    }
}
