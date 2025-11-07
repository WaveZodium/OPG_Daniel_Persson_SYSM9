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

        // Local helpers for consistency
        Paragraph Header(string text, double top = 12, double bottom = 4) =>
            new() { Margin = new Thickness(0, top, 0, bottom), Inlines = { new Bold(new Run(text)) } };
        List BulletList() => new() { MarkerStyle = TextMarkerStyle.Disc };
        List CircleList() => new() { MarkerStyle = TextMarkerStyle.Circle };
        List NumberList() => new() { MarkerStyle = TextMarkerStyle.Decimal };

        // Title
        doc.Blocks.Add(new Paragraph {
            Margin = new Thickness(0, 0, 0, 10),
            Inlines = { new Bold(new Run("CookMaster – Usage Guide")) }
        });

        // Overview
        doc.Blocks.Add(new Paragraph(new Run(
            "CookMaster lets users create, view, copy, edit and manage recipes. Admins additionally manage users and see all recipes. This guide summarizes common actions, filtering, dialogs, and behavior.")));

        // Quick start
        doc.Blocks.Add(Header("Quick start", top: 10));
        var quick = NumberList();
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Sign in (or Register a new account)."))));
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Use the search / filters if you have many recipes."))));
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Click Add Recipe to create one."))));
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Select a recipe row → View Recipe to inspect or edit."))));
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Select a recipe → Copy Recipe to create a variant."))));
        quick.ListItems.Add(new ListItem(new Paragraph(new Run("Use Logout when finished."))));
        doc.Blocks.Add(quick);

        // Roles
        doc.Blocks.Add(Header("User roles"));
        var roles = BulletList();
        roles.ListItems.Add(new ListItem(new Paragraph(new Run("Standard user: Sees only own recipes; can add, copy, edit, delete their recipes."))));
        roles.ListItems.Add(new ListItem(new Paragraph(new Run("Admin: Sees all recipes; can delete any; can change owner; can copy any recipe; can open user list."))));
        doc.Blocks.Add(roles);

        // Sign in & account
        doc.Blocks.Add(Header("Sign in & account"));
        var signin = BulletList();
        signin.ListItems.Add(new ListItem(new Paragraph(new Run("Enter Username + Password then Sign In."))));
        signin.ListItems.Add(new ListItem(new Paragraph(new Run("Register: Creates a new account."))));
        signin.ListItems.Add(new ListItem(new Paragraph(new Run("Forgot password: Recovery flow (if implemented)."))));
        signin.ListItems.Add(new ListItem(new Paragraph(new Run("Click your displayed username to open user details."))));
        doc.Blocks.Add(signin);

        // Recipe list window
        doc.Blocks.Add(Header("Recipe list window"));
        doc.Blocks.Add(new Paragraph(new Run("Shows a grid of recipes. Select a row to enable context actions.")));

        var columns = CircleList();
        columns.ListItems.Add(new ListItem(new Paragraph(new Run("Created – timestamp (yyyy-MM-dd HH:mm)."))));
        columns.ListItems.Add(new ListItem(new Paragraph(new Run("Recipe Name – title."))));
        columns.ListItems.Add(new ListItem(new Paragraph(new Run("Category – classification."))));
        columns.ListItems.Add(new ListItem(new Paragraph(new Run("Owner – creator (visible; relevant to admins)."))));
        doc.Blocks.Add(columns);

        doc.Blocks.Add(Header("Actions", top: 6));
        var actions = BulletList();
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("Add Recipe – open creation dialog."))));
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("View Recipe – inspect/edit selected recipe."))));
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("Copy Recipe – create a new recipe pre-filled from the selected one (variant drafting)."))));
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("Delete Recipe – remove selected (owner or admin)."))));
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("List Users – admin user management view."))));
        actions.ListItems.Add(new ListItem(new Paragraph(new Run("Help / Info icon – reopen this guide."))));
        doc.Blocks.Add(actions);

        // Filtering & searching
        doc.Blocks.Add(Header("Filtering & searching"));
        var filtering = BulletList();
        filtering.ListItems.Add(new ListItem(new Paragraph(new Run("Search box: case-insensitive match against title, category name, or owner username."))));
        filtering.ListItems.Add(new ListItem(new Paragraph(new Run("Category filter: choose a category or blank (null) to show all."))));
        filtering.ListItems.Add(new ListItem(new Paragraph(new Run("Date filter: picks recipes created on that calendar day (local date)."))));
        filtering.ListItems.Add(new ListItem(new Paragraph(new Run("Filters combine (AND). Clear Filters resets all to show full list."))));
        filtering.ListItems.Add(new ListItem(new Paragraph(new Run("Admin sees all recipes; standard users see only their own before filters apply."))));
        doc.Blocks.Add(filtering);

        // Add recipe dialog
        doc.Blocks.Add(Header("Add recipe dialog"));
        var add = NumberList();
        add.ListItems.Add(new ListItem(new Paragraph(new Run("Enter Title."))));
        add.ListItems.Add(new ListItem(new Paragraph(new Run("Ingredients: type ingredient → Add. Select + Remove to delete."))));
        add.ListItems.Add(new ListItem(new Paragraph(new Run("Instructions: multi-line text; wrapping enabled."))));
        add.ListItems.Add(new ListItem(new Paragraph(new Run("Select Category."))));
        add.ListItems.Add(new ListItem(new Paragraph(new Run("(If opened as Copy) Fields are pre-filled; adjust as needed."))));
        add.ListItems.Add(new ListItem(new Paragraph(new Run("Click Add Recipe to save or Cancel to abort."))));
        doc.Blocks.Add(add);

        // Recipe detail dialog
        doc.Blocks.Add(Header("Recipe detail dialog"));
        var detail = BulletList();
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Edit title, ingredients, instructions, category."))));
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Copy Recipe: creates a separate new recipe using current fields (original unchanged)."))));
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Owner (admins only) can be reassigned."))));
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Save applies changes immediately."))));
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Close: prompts if unsaved changes (Save / Discard / Cancel)."))));
        detail.ListItems.Add(new ListItem(new Paragraph(new Run("Delete: confirmation required (owner/admin)."))));
        doc.Blocks.Add(detail);

        // Copying behavior
        doc.Blocks.Add(Header("Copying recipes"));
        var copy = BulletList();
        copy.ListItems.Add(new ListItem(new Paragraph(new Run("From list: Select a recipe → Copy Recipe to open Add dialog pre-filled."))));
        copy.ListItems.Add(new ListItem(new Paragraph(new Run("From detail: Use Copy to branch a variation while keeping the original."))));
        copy.ListItems.Add(new ListItem(new Paragraph(new Run("Copied recipe fields: title, ingredients, instructions, category. Timestamps reset on save."))));
        copy.ListItems.Add(new ListItem(new Paragraph(new Run("Original recipe is not modified; you must click Add/Save in the dialog to persist the copy."))));
        doc.Blocks.Add(copy);

        // User management (admin)
        doc.Blocks.Add(Header("User management (admin)"));
        var um = BulletList();
        um.ListItems.Add(new ListItem(new Paragraph(new Run("List Users window shows accounts."))));
        um.ListItems.Add(new ListItem(new Paragraph(new Run("View user details (read-only unless extended)."))));
        doc.Blocks.Add(um);

        // Data & validation
        doc.Blocks.Add(Header("Data & validation"));
        var data = BulletList();
        data.ListItems.Add(new ListItem(new Paragraph(new Run("Mandatory: title, ≥1 ingredient, instructions, category."))));
        data.ListItems.Add(new ListItem(new Paragraph(new Run("Duplicate ingredients can be manually removed; no silent merge."))));
        data.ListItems.Add(new ListItem(new Paragraph(new Run("Timestamps use local time for display."))));
        doc.Blocks.Add(data);

        // Safety & confirmations
        doc.Blocks.Add(Header("Safety & confirmations"));
        var safe = BulletList();
        safe.ListItems.Add(new ListItem(new Paragraph(new Run("Delete operations always ask for confirmation."))));
        safe.ListItems.Add(new ListItem(new Paragraph(new Run("Unsaved changes prompt prevents accidental loss."))));
        doc.Blocks.Add(safe);

        // Keyboard shortcuts
        doc.Blocks.Add(Header("Keyboard shortcuts"));
        var keys = BulletList();
        keys.ListItems.Add(new ListItem(new Paragraph(new Run("Esc: Close dialogs (if no pending confirmation)."))));
        keys.ListItems.Add(new ListItem(new Paragraph(new Run("Enter: Activates default button (Add / Save) unless focus is in multi-line text."))));
        doc.Blocks.Add(keys);

        // Troubleshooting / tips
        doc.Blocks.Add(Header("Troubleshooting / tips"));
        var tips = BulletList();
        tips.ListItems.Add(new ListItem(new Paragraph(new Run("Cannot see other recipes? You may be a standard user (only own recipes)."))));
        tips.ListItems.Add(new ListItem(new Paragraph(new Run("Actions disabled? Ensure a recipe row is selected."))));
        tips.ListItems.Add(new ListItem(new Paragraph(new Run("Filters hiding expected recipes? Click Clear Filters."))));
        tips.ListItems.Add(new ListItem(new Paragraph(new Run("Need a variant? Use Copy Recipe instead of editing the original."))));
        tips.ListItems.Add(new ListItem(new Paragraph(new Run("No ingredients? Add at least one before saving."))));
        doc.Blocks.Add(tips);

        // Final note
        doc.Blocks.Add(new Paragraph(new Run("Reopen this window via the info icon any time. Good luck and enjoy cooking!")));

        return doc;
    }
}
