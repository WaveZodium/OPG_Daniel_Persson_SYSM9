namespace CookMaster.Models;

public class AdminUser : User {

    public AdminUser(string username, string password, Country country, string email, string securityQuestion, string securityAnswer) :
        base(username, password, UserRole.Admin, country, email, securityQuestion, securityAnswer) { }

    public void RemoveAnyRecipe(Recipe recipe) {
        // Implementation for removing any recipe
    }

    public void ViewAllRecipes() {
        // Implementation for viewing all recipes
    }
}
