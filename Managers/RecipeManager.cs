using CookMaster.Models;

namespace CookMaster.Managers;

public class RecipeManager {
    private readonly UserManager _userManager;

    public List<Recipe> Recipes { get; set; } = new List<Recipe>();

    public enum RecipeFilter {
        ByName,
        ByIngredient,
        ByCategory,
        ByCookingTime
    }

    public RecipeManager(UserManager userManager) {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        SeedDefaults();
    }

    public void AddRecipe(Recipe recipe) {
        if (recipe == null) return;
        Recipes.Add(recipe);
    }

    public bool RemoveRecipe(Recipe recipe) {
        if (recipe == null) return false;
        return Recipes.Remove(recipe);
    }

    public IEnumerable<Recipe> GetAllRecipes() => Recipes.ToList();

    public IEnumerable<Recipe> GetByOwner(User owner) {
        if (owner == null) return Enumerable.Empty<Recipe>();
        return Recipes.Where(r => r.Owner != null && r.Owner.Id == owner.Id).ToList();
    }

    public IEnumerable<Recipe> GetByUser(User user) {
        if (user == null) return Enumerable.Empty<Recipe>();
        return Recipes.Where(r => r.Owner != null && r.Owner.Id == user.Id).ToList();
    }

    public bool RecipeExists(string title) {
        if (string.IsNullOrWhiteSpace(title)) return false;
        return Recipes.Any(r => string.Equals(r.Title, title.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    // Idempotent seeding using the injected UserManager
    public void SeedDefaults() {
        var user = _userManager.FindUser("user");
        if (user == null) return;

        if (!RecipeExists("Pancakes")) {
            AddRecipe(new Recipe("Pancakes", new List<string> { "Flour", "Eggs", "Milk" }, "Mix and fry", RecipeCategory.Breakfast, DateTime.Now, DateTime.Now, user));
        }

        if (!RecipeExists("Pasta Carbonara")) {
            AddRecipe(new Recipe("Pasta Carbonara", new List<string> { "Pasta", "Eggs", "Parmesan cheese" }, "Cook pasta and mix with eggs and cheese", RecipeCategory.MainCourse, DateTime.Now, DateTime.Now, user));
        }
        if (!RecipeExists("Spaghetti Bolognese")) {
            // seed more recipes as user2
            AddRecipe(new Recipe("Spaghetti Bolognese", new List<string> { "Spaghetti", "Ground beef", "Tomato sauce" }, "Cook spaghetti and prepare sauce", RecipeCategory.MainCourse, DateTime.Now, DateTime.Now, user));
        }
        if (!RecipeExists("Chicken Curry")) {
            AddRecipe(new Recipe("Chicken Curry", new List<string> { "Chicken", "Curry powder", "Coconut milk" }, "Cook chicken and simmer in curry sauce", RecipeCategory.MainCourse, DateTime.Now, DateTime.Now, user));
        }

        var user2 = _userManager.FindUser("user2");

        if (user2 != null && !RecipeExists("Caesar Salad")) {
            AddRecipe(new Recipe("Caesar Salad", new List<string> { "Lettuce", "Croutons", "Caesar dressing" }, "Toss ingredients together", RecipeCategory.Salad, DateTime.Now, DateTime.Now, user2));
            // seed more recipes as user2
        }
        if (user2 != null && !RecipeExists("Tomato Soup")) {
            AddRecipe(new Recipe("Tomato Soup", new List<string> { "Tomatoes", "Onion", "Garlic", "Vegetable broth" }, "Cook and blend ingredients", RecipeCategory.Soup, DateTime.Now, DateTime.Now, user2));
        }
        if (user2 != null && !RecipeExists("Grilled Cheese Sandwich")) {
            AddRecipe(new Recipe("Grilled Cheese Sandwich", new List<string> { "Bread", "Cheese", "Butter" }, "Assemble and grill the sandwich", RecipeCategory.Snack, DateTime.Now, DateTime.Now, user2));
        }


        var admin = _userManager.FindUser("admin");

        if (admin != null && !RecipeExists("Chocolate Cake")) {
            AddRecipe(new Recipe("Chocolate Cake", new List<string> { "Flour", "Cocoa powder", "Sugar", "Eggs", "Butter" }, "Mix ingredients and bake", RecipeCategory.Dessert, DateTime.Now, DateTime.Now, admin));
        }


    }
}
