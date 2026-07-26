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
/// Two facing pages of the book, or the burned back section.
///
/// The burned spread carries no pages at all — it is the end of the book, not a
/// page of it, which is why <see cref="RecipeBookRules.CompilePages"/> never
/// emits it and <see cref="RecipeBookRules.CompileSpreads"/> always appends it.
/// </summary>
public readonly struct BookSpread
{
    public readonly BookPage Left;
    public readonly BookPage Right;
    public readonly bool HasRight;
    public readonly bool IsBurnedSection;

    public BookSpread(BookPage left, BookPage right, bool hasRight, bool isBurnedSection)
    {
        Left = left;
        Right = right;
        HasRight = hasRight;
        IsBurnedSection = isBurnedSection;
    }

    public static BookSpread Burned() => new BookSpread(default, default, false, true);
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

    /// <summary>
    /// Groups pages into facing pairs and appends the burned section as the final
    /// spread. An odd page count leaves the last right-hand page blank rather than
    /// pulling the burned section forward — a book does not reflow.
    /// </summary>
    public static List<BookSpread> CompileSpreads(IReadOnlyList<BookPage> pages)
    {
        var spreads = new List<BookSpread>();

        if (pages != null)
        {
            for (int i = 0; i < pages.Count; i += 2)
            {
                bool hasRight = i + 1 < pages.Count;
                spreads.Add(new BookSpread(
                    pages[i],
                    hasRight ? pages[i + 1] : default,
                    hasRight,
                    false));
            }
        }

        spreads.Add(BookSpread.Burned());
        return spreads;
    }

    public static int ClampSpreadIndex(int index, int spreadCount)
    {
        if (spreadCount <= 0) return 0;
        if (index < 0) return 0;
        if (index >= spreadCount) return spreadCount - 1;
        return index;
    }
}
