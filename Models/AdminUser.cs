using System;

namespace CookMaster.Models;

public class AdminUser : User {

    public AdminUser(string username, string password, Country country) : base(username, password, UserRole.Admin, country) { }

    public void RemoveAnyRecipe(Recipe recipe) {
        // Implementation for removing any recipe
    }

    public void ViewAllRecipes() {
        // Implementation for viewing all recipes
    }
}
