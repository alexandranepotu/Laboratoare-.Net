using System.Text.RegularExpressions;
using FluentValidation;
using laboratorul4.Data;
using laboratorul4.Features.Orders;
using laboratorul4.Features.Orders.Dtos;
using Microsoft.EntityFrameworkCore;


namespace laboratorul4.Validators;

public class CreateOrderProfileValidator : AbstractValidator<CreateOrderProfileRequest>
{
    private readonly OrderDbContext _db;
    private readonly ILogger<CreateOrderProfileValidator> _logger;
    
    private static readonly HashSet<string> InappropriateWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "spam", "scam", "fake", "fraud", "adult", "explicit", "violent", "hate"
    };

    private static readonly HashSet<string> ChildrenRestrictedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "violence", "war", "death", "blood", "murder", "kill", "weapon", "gun",
        "alcohol", "drug", "smoking", "gambling", "horror", "scary", "terror"
    };

    private static readonly HashSet<string> TechnicalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "programming", "software", "development", "algorithm", "database", "code", "coding",
        "computer", "technology", "engineering", "technical", "advanced", "guide", "manual",
        "system", "architecture", "design", "pattern", "framework", "api", "web", "cloud",
        "data", "structure", "network", "security", "python", "java", "c#", "javascript"
    };

    public CreateOrderProfileValidator(OrderDbContext db, ILogger<CreateOrderProfileValidator> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Titlu
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required and cannot be empty.")
            .MinimumLength(1).WithMessage("Title must be at least 1 character long.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .Must(BeValidTitle).WithMessage("Title contains inappropriate content.")
            .MustAsync(BeUniqueTitle).WithMessage("A book with this title by the same author already exists.");

        // Autor
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author name is required and cannot be empty.")
            .MinimumLength(2).WithMessage("Author name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Author name cannot exceed 100 characters.")
            .Must(BeValidAuthorName).WithMessage("Author name must contain only letters, spaces, hyphens, apostrophes, and dots.");

        // ISBN
        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required and cannot be empty.")
            .Must(BeValidISBN).WithMessage("ISBN must be a valid format (10 or 13 digits, may contain hyphens).")
            .MustAsync(BeUniqueISBN).WithMessage("An order with this ISBN already exists.");

        // Categorie
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value.");

        // Pret
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThan(10000).WithMessage("Price must be less than $10,000.");

        // Data publicarii
        RuleFor(x => x.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.")
            .GreaterThanOrEqualTo(new DateTime(1400, 1, 1)).WithMessage("Published date cannot be before year 1400.");

        // Stoc
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

        //Coperta
        RuleFor(x => x.CoverImageUrl)
            .Must(BeValidImageUrl!)
            .WithMessage("Cover image URL must be a valid HTTP/HTTPS image URL ending with .jpg, .jpeg, .png, .gif, or .webp.")
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl));

        // Technical: Pret minim $20.00
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(20.00m)
            .WithMessage("Technical books must have a minimum price of $20.00.")
            .When(x => x.Category == OrderCategory.Technical);

        // Technical: Titlul trebuie sa contina cuvinte cheie tehnice
        RuleFor(x => x.Title)
            .Must(ContainsTechnicalKeywords)
            .WithMessage("Technical book title must contain at least one technical keyword (e.g., programming, software, development, algorithm, etc.).")
            .When(x => x.Category == OrderCategory.Technical);

        // Technical: Publicata in ultimii 5 ani
        RuleFor(x => x.PublishedDate)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.AddYears(-5))
            .WithMessage("Technical books must be published within the last 5 years.")
            .When(x => x.Category == OrderCategory.Technical);

        // Children's: Pret maxim $50.00
        RuleFor(x => x.Price)
            .LessThanOrEqualTo(50.00m)
            .WithMessage("Children's books cannot exceed $50.00.")
            .When(x => x.Category == OrderCategory.Children);

        // Children's: Titlu fara cuvinte nepotrivite
        RuleFor(x => x.Title)
            .Must(BeAppropriateForChildren)
            .WithMessage("Children's book title contains inappropriate words.")
            .When(x => x.Category == OrderCategory.Children);

        // Fiction: Autor minim 5 caractere
        RuleFor(x => x.Author)
            .MinimumLength(5)
            .WithMessage("Fiction books require full author name (minimum 5 characters).")
            .When(x => x.Category == OrderCategory.Fiction);

        // Cross-field: Expensive orders (>$100) -> Stock ≤ 20
        RuleFor(x => x.StockQuantity)
            .LessThanOrEqualTo(20)
            .WithMessage("Expensive orders (over $100) must have stock quantity of 20 or fewer units.")
            .When(x => x.Price > 100.00m);

        RuleFor(x => x)
            .MustAsync(PassBusinessRules)
            .WithMessage("Order does not pass complex business rules validation.");
    }


    private bool BeValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return true;

        var words = title.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        return !words.Any(word => InappropriateWords.Contains(word));
    }

    private async Task<bool> BeUniqueTitle(CreateOrderProfileRequest request, string title, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating title uniqueness for Title={Title}, Author={Author}", title, request.Author);

        var exists = await _db.Orders.AnyAsync(
            o => o.Title == title && o.Author == request.Author,
            cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Duplicate title found: Title={Title}, Author={Author}", title, request.Author);
        }

        return !exists;
    }

    private bool BeValidAuthorName(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            return true;

        var regex = new Regex(@"^[\p{L}\s\-'.]+$", RegexOptions.None, TimeSpan.FromSeconds(1));
        return regex.IsMatch(author);
    }

    private bool BeValidISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return true;

        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "").Trim();

        if (cleanIsbn.Length != 10 && cleanIsbn.Length != 13)
            return false;

        return cleanIsbn.All(c => char.IsDigit(c) ||
                                  (cleanIsbn.Length == 10 && (c == 'X' || c == 'x')));
    }

    private async Task<bool> BeUniqueISBN(string isbn, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating ISBN uniqueness for ISBN={ISBN}", isbn);

        var exists = await _db.Orders.AnyAsync(o => o.ISBN == isbn, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Duplicate ISBN found: ISBN={ISBN}", isbn);
        }

        return !exists;
    }

    private bool BeValidImageUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return true;

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var path = uri.AbsolutePath.ToLowerInvariant();

        return validExtensions.Any(ext => path.EndsWith(ext));
    }

    private async Task<bool> PassBusinessRules(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating complex business rules for ISBN={ISBN}, Category={Category}",
            request.ISBN, request.Category);

        // Rule 1: Daily order addition limit (max 5 per day)
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var dailyCount = await _db.Orders.CountAsync(
            o => o.CreatedAt >= today && o.CreatedAt < tomorrow,
            cancellationToken);

        if (dailyCount >= 5)
        {
            _logger.LogWarning("Daily order limit reached (max 5 orders/day)");
            return false;
        }

        // Rule 2: Technical orders minimum price check (>= $20.00)
        if (request.Category == OrderCategory.Technical && request.Price < 20.00m)
        {
            _logger.LogWarning("Technical book price too low: Price={Price}", request.Price);
            return false;
        }

        // Rule 3: Children's content restrictions
        if (request.Category == OrderCategory.Children && !BeAppropriateForChildren(request.Title))
        {
            _logger.LogWarning("Children's book contains restricted words.");
            return false;
        }

        // Rule 4: High-value order stock limit (>$500 = max 10 stock)
        if (request.Price > 500.00m && request.StockQuantity > 10)
        {
            _logger.LogWarning(
                "High-value books cannot have more than 10 in stock. Price={Price}, Stock={Stock}",
                request.Price, request.StockQuantity);
            return false;
        }

        _logger.LogInformation("All business rules passed for ISBN={ISBN}", request.ISBN);
        return true;
    }


    private bool ContainsTechnicalKeywords(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        var words = title.Split(new[] { ' ', ',', '.', '!', '?', '-', '_' },
            StringSplitOptions.RemoveEmptyEntries);

        return words.Any(word => TechnicalKeywords.Contains(word));
    }

    private bool BeAppropriateForChildren(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return true;

        var words = title.Split(new[] { ' ', ',', '.', '!', '?', '-', '_' },
            StringSplitOptions.RemoveEmptyEntries);

        return !words.Any(word => ChildrenRestrictedWords.Contains(word));
    }
}
