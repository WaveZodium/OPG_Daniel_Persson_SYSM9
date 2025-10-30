namespace CookMaster.Models;

public class Recipe {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new List<string>();
    public string Instructions { get; set; } = string.Empty;
    public RecipeCategory Category { get; set; } = RecipeCategory.Other;
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
    public User? Owner { get; set; }

    public Recipe() {
        
    }

    public Recipe(string title, List<string> ingredients, string instructions, RecipeCategory category, DateTime created, DateTime updated, User owner) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
        Created = created;
        Updated = updated;
        Owner = owner;
    }

    public void EditRecipe(string title, List<string> ingredients, string instructions, RecipeCategory category, User owner) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
        Owner = owner;
        Updated = DateTime.UtcNow;
    }

    public void UpdateOwner(User newOwner) {
        Owner = newOwner;
    }

    public Recipe CopyRecipe() {
        return new Recipe(Title, new List<string>(Ingredients), Instructions, Category, Created, Updated, Owner!);
    }

}
