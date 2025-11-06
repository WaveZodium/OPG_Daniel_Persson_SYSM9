using System;
using System.Collections.Generic;

namespace CookMaster.Models;

public class Recipe {
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public string Instructions { get; set; } = string.Empty;
    public RecipeCategory Category { get; set; } = RecipeCategory.Other;

    public DateTime Created { get; init; }
    public DateTime Updated { get; set; }
    public User? Owner { get; set; }

    public Recipe() {
        var now = DateTime.UtcNow;
        Created = now;
        Updated = now;
    }

    public Recipe(string title, List<string> ingredients, string instructions, RecipeCategory category, DateTime created, DateTime updated, User owner) {
        Title = title;
        Ingredients = ingredients ?? new();
        Instructions = instructions ?? string.Empty;
        Category = category;
        Created = ToUtc(created);
        Updated = ToUtc(updated);
        Owner = owner;
    }

    public void EditRecipe(string title, List<string> ingredients, string instructions, RecipeCategory category) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
        Updated = DateTime.UtcNow;
    }

    public void EditRecipe(string title, List<string> ingredients, string instructions, RecipeCategory category, User owner) {
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Category = category;
        Owner = owner;
        Updated = DateTime.UtcNow;
    }

    public void UpdateOwner(User newOwner) => Owner = newOwner;

    public Recipe CopyRecipe() =>
        new Recipe(Title, new List<string>(Ingredients), Instructions, Category, Created, Updated, Owner!);

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind switch {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime()
        };
}
