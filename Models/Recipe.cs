namespace CookMaster.Models;

public class Recipe {
    private Guid Id { get; init; } = Guid.NewGuid();
    private string Title { get; set; } = string.Empty;
    private string Ingredients { get; set; } = string.Empty;
    private string Instructions { get; set; } = string.Empty;
    private RecipeCategory Category { get; set; } = RecipeCategory.Other;
    private DateTime Date { get; init; } = DateTime.UtcNow;
    private User CreatedBy { get; init; } = new User();

    public void EditRecipe() {
    }

    public void CopyRecipe() {
    }

}
