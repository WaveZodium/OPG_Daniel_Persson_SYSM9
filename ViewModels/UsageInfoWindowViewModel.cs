using System;
using System.Windows;
using System.Windows.Documents;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UsageInfoWindowViewModel : ViewModelBase {
    // 3) Events
    public event Action<bool>? RequestClose;

    // 4) Constructors
    public UsageInfoWindowViewModel() {
        UsageInfoDocument = BuildDefaultDocument();
        PerformCloseCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand PerformCloseCommand { get; }

    // 6) Bindable state (editable input)
    private FlowDocument _usageInfoDocument = new();
    public FlowDocument UsageInfoDocument {
        get => _usageInfoDocument;
        set => Set(ref _usageInfoDocument, value);
    }

    // 10) Private helpers
    private static FlowDocument BuildDefaultDocument() {
        var doc = new FlowDocument {
            PagePadding = new Thickness(8),
            FontSize = SystemFonts.MessageFontSize,
            FontFamily = SystemFonts.MessageFontFamily
        };

        // Title
        var title = new Paragraph { Margin = new Thickness(0, 0, 0, 8) };
        title.Inlines.Add(new Bold(new Run("CookMaster - Usage Guide")));
        doc.Blocks.Add(title);

        // Intro
        doc.Blocks.Add(new Paragraph(new Run(
            "This guide explains the common tasks in CookMaster: signing in, working with recipes, and user management.")));

        // Sign in and account actions
        var signinHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        signinHeader.Inlines.Add(new Bold(new Run("Sign in and account actions")));
        doc.Blocks.Add(signinHeader);

        var signinList = new List { MarkerStyle = TextMarkerStyle.Disc };
        signinList.ListItems.Add(new ListItem(new Paragraph(new Run("Enter Username and Password, then choose Sign In."))));
        signinList.ListItems.Add(new ListItem(new Paragraph(new Run("Use \"Register\" to create a new account if you don’t have one."))));
        signinList.ListItems.Add(new ListItem(new Paragraph(new Run("Use \"Forgot password\" to recover access if needed."))));
        doc.Blocks.Add(signinList);

        // Recipe list window
        var listHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        listHeader.Inlines.Add(new Bold(new Run("Recipe list window")));
        doc.Blocks.Add(listHeader);

        doc.Blocks.Add(new Paragraph(new Run(
            "The grid shows your recipes (or all recipes if you are an admin). Select a row to enable actions.")));

        var colsList = new List { MarkerStyle = TextMarkerStyle.Circle };
        colsList.ListItems.Add(new ListItem(new Paragraph(new Run("Created — when the recipe was created (yyyy-MM-dd HH:mm)."))));
        colsList.ListItems.Add(new ListItem(new Paragraph(new Run("Recipe Name — the title."))));
        colsList.ListItems.Add(new ListItem(new Paragraph(new Run("Category — the recipe category."))));
        colsList.ListItems.Add(new ListItem(new Paragraph(new Run("Owner — who created the recipe."))));
        doc.Blocks.Add(colsList);

        var actionsHeader = new Paragraph { Margin = new Thickness(0, 8, 0, 4) };
        actionsHeader.Inlines.Add(new Bold(new Run("Actions")));
        doc.Blocks.Add(actionsHeader);

        var actionsList = new List { MarkerStyle = TextMarkerStyle.Disc };
        actionsList.ListItems.Add(new ListItem(new Paragraph(new Run("Add Recipe — open a dialog to create a new recipe."))));
        actionsList.ListItems.Add(new ListItem(new Paragraph(new Run("View Recipe — open details for the selected recipe."))));
        actionsList.ListItems.Add(new ListItem(new Paragraph(new Run("Delete Recipe — delete the selected recipe (owner or admin only)."))));
        actionsList.ListItems.Add(new ListItem(new Paragraph(new Run("List Users — view all users (admins only)."))));
        doc.Blocks.Add(actionsList);

        // User UI
        var userHeader = new Paragraph { Margin = new Thickness(0, 8, 0, 4) };
        userHeader.Inlines.Add(new Bold(new Run("User UI")));
        doc.Blocks.Add(userHeader);

        var userList = new List { MarkerStyle = TextMarkerStyle.Disc };
        userList.ListItems.Add(new ListItem(new Paragraph(new Run("Logged in as: Click your username to open your user details."))));
        userList.ListItems.Add(new ListItem(new Paragraph(new Run("Use Logout to sign out and return to the sign-in window."))));
        userList.ListItems.Add(new ListItem(new Paragraph(new Run("Use the info icon or row to reopen this help window."))));
        doc.Blocks.Add(userList);

        // Add recipe
        var addHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        addHeader.Inlines.Add(new Bold(new Run("Add recipe")));
        doc.Blocks.Add(addHeader);

        var addList = new List { MarkerStyle = TextMarkerStyle.Decimal };
        addList.ListItems.Add(new ListItem(new Paragraph(new Run("Enter Title."))));
        addList.ListItems.Add(new ListItem(new Paragraph(new Run("Ingredients: type an ingredient and click Add. Select an item and click Remove to delete it."))));
        addList.ListItems.Add(new ListItem(new Paragraph(new Run("Write the Instructions (multi-line, wrapping is enabled)."))));
        addList.ListItems.Add(new ListItem(new Paragraph(new Run("Choose a Category."))));
        addList.ListItems.Add(new ListItem(new Paragraph(new Run("Click Add Recipe to save, or Cancel to close without saving."))));
        doc.Blocks.Add(addList);

        // Recipe details
        var detailsHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        detailsHeader.Inlines.Add(new Bold(new Run("Recipe details")));
        doc.Blocks.Add(detailsHeader);

        var detailsList = new List { MarkerStyle = TextMarkerStyle.Disc };
        detailsList.ListItems.Add(new ListItem(new Paragraph(new Run("Edit fields (title, ingredients, instructions, category)."))));
        detailsList.ListItems.Add(new ListItem(new Paragraph(new Run("Save — applies your changes to the original recipe."))));
        detailsList.ListItems.Add(new ListItem(new Paragraph(new Run("Close — if there are unsaved changes, you’ll be prompted to Save, Discard, or Cancel."))));
        detailsList.ListItems.Add(new ListItem(new Paragraph(new Run("Delete — removes the recipe after confirmation (owner or admin)."))));
        detailsList.ListItems.Add(new ListItem(new Paragraph(new Run("Admins can change the Owner in this view."))));
        doc.Blocks.Add(detailsList);

        // Admin features
        var adminHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        adminHeader.Inlines.Add(new Bold(new Run("Admin features")));
        doc.Blocks.Add(adminHeader);

        var adminList = new List { MarkerStyle = TextMarkerStyle.Disc };
        adminList.ListItems.Add(new ListItem(new Paragraph(new Run("Recipe list shows all users’ recipes."))));
        adminList.ListItems.Add(new ListItem(new Paragraph(new Run("List Users opens the user management window."))));
        doc.Blocks.Add(adminList);

        // Tips
        var tipsHeader = new Paragraph { Margin = new Thickness(0, 12, 0, 4) };
        tipsHeader.Inlines.Add(new Bold(new Run("Tips")));
        doc.Blocks.Add(tipsHeader);

        var tipsList = new List { MarkerStyle = TextMarkerStyle.Disc };
        tipsList.ListItems.Add(new ListItem(new Paragraph(new Run("Select a recipe row to enable View and Delete."))));
        tipsList.ListItems.Add(new ListItem(new Paragraph(new Run("If your list is empty, you may not be an admin; non-admins see only their own recipes."))));
        tipsList.ListItems.Add(new ListItem(new Paragraph(new Run("Confirmation dialogs help prevent accidental deletes and data loss."))));
        doc.Blocks.Add(tipsList);

        return doc;
    }
}
