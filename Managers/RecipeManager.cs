using System;
using System.Collections.Generic;
using System.Linq;
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

    public IEnumerable<Recipe> GetByUser(User user) {
        if (user == null) return Enumerable.Empty<Recipe>();
        return Recipes.Where(r => r.CreatedBy != null && r.CreatedBy.Id == user.Id).ToList();
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
            AddRecipe(new Recipe("Pancakes", new List<string>{ "Flour", "Eggs", "Milk" }, "Mix and fry", RecipeCategory.Breakfast, user));
        }

        if (!RecipeExists("Pasta Carbonara")) {
            AddRecipe(new Recipe("Pasta Carbonara", new List<string>{ "Pasta", "Eggs", "Parmesan cheese" }, "Cook pasta and mix with eggs and cheese", RecipeCategory.MainCourse, user));
        }
    }
}
