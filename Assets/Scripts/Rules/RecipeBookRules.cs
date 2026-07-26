using System;
using System.Collections.Generic;

/// <summary>
/// One page of the player's inherited recipe book.
///
/// A torn page carries no <see cref="RecipeData"/> at all. That is the point:
/// the book shows the player that a page is missing, never what was on it.
/// Naming an undiscovered recipe would spend the discovery hook to buy the
/// cellar hook, so the type makes the leak impossible rather than relying on
/// the draw code to remember.
/// </summary>
public readonly struct BookPage
{
    public readonly int PageNumber;
    public readonly RecipeData Recipe;

    public bool IsLegible => Recipe != null;

    public BookPage(int pageNumber, RecipeData recipe)
    {
        PageNumber = pageNumber;
        Recipe = recipe;
    }
}

/// <summary>
/// The recipe book: pages are recipes. The player inherits it already damaged,
/// with a single legible page.
///
/// Pages become legible through the normal discovery path. The burned section
/// does not — no building, purchase, or discovery ever restores it. It is the
/// cellar's seed, and it is deliberately not a recipe so that a player never
/// waits on an unlock that is not coming.
/// </summary>
public static class RecipeBookRules
{
    /// <summary>
    /// What survives of the burned section. Three scraps, no more: name a place,
    /// imply people, imply an ending.
    /// </summary>
    public static readonly IReadOnlyList<string> BurnedScraps = new[]
    {
        "...the copper wants a slower fire than...",
        "...and we took the rest below, because...",
        "...if you are reading this then they did not..."
    };

    /// <summary>
    /// Builds the book in fixed page order. Page numbers never shift, so a page
    /// the player has seen torn becomes legible in place — the way a book behaves.
    /// </summary>
    public static List<BookPage> CompilePages(
        IReadOnlyList<RecipeData> recipes,
        Func<RecipeData, bool> isDiscovered)
    {
        var pages = new List<BookPage>();
        if (recipes == null || isDiscovered == null) return pages;

        for (int i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            bool legible = recipe != null && isDiscovered(recipe);
            pages.Add(new BookPage(i + 1, legible ? recipe : null));
        }

        return pages;
    }
}
