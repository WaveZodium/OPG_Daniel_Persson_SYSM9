namespace CookMaster.Models;

public class Recipe {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new List<string>();
    public string Instructions { get; set; } = string.Empty;
    public RecipeCategory Category { get; set; } = RecipeCategory.Other;
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public User? CreatedBy { get; init; }

    public Recipe() {
        
    }

    public Recipe(string title, List<string> ingredients, string instructions, RecipeCategory category, User createdBy) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
        CreatedBy = createdBy;
    }

    public void EditRecipe(string title, List<string> ingredients, string instructions, RecipeCategory category) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
    }

    public Recipe CopyRecipe() {
        return new Recipe(Title, new List<string>(Ingredients), Instructions, Category, CreatedBy);
    }

}
